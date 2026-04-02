using AML.Core.ServiceContract.AI;
using AML.DTO.DTO.AI;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AML.Core.Service.AI
{
    public class OllamaAIService : BaseService, IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _ollamaUrl;
        private readonly string _modelName;
        private readonly string _modelName;

        public OllamaAIService(IConfiguration configuration) : base(configuration)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(configuration["AI:OllamaUrl"] ?? "http://localhost:11434/"),
                Timeout = TimeSpan.FromMinutes(2)
            };
            _modelName = configuration["AI:ModelName"] ?? "llama3.1:8b";
        }

        private string GetSystemPrompt(string context)
        {
            return $@"
You are an intelligent AML/KYC assistant for the 'Lemon WatchDog' system.
Your goal is to provide intelligent, professional, and business-focused summaries based on the provided context.

### CRITICAL RULES:
1. **NO STATUS CODES**: NEVER show numeric status codes (e.g., 'Status Code 0', 'Code 2') to the user. Instead, use the descriptive name (e.g., 'Pending', 'Approved').
2. **NO TECHNICAL OUTPUT**: Do not show JSON, SQL, or programming code snippets.
3. **RICH FORMATTING**: Use **standard Markdown**. Use `### Header` for sections, `**bold**` for key terms, and `* bullet points`. **NEVER** use long strings of dashes (e.g., `-------`) as separators.
4. **SPACING**: You MUST use **double newlines** (`\n\n`) between paragraphs and sections. This is critical for the Markdown renderer to correctly display your response without clumping.
5. **SECURITY**: Only answer based on the provided context. If a user asks for data not in the context, politely refuse.

### Business Process Reference (DO NOT SHOW CODES IN RESPONSE):
- Pending (0/6): Initial state or batch scheduler.
- Approved (2): Final state, all shareholders must also be approved.
- Rejected (3): Final state.
- Senior Management (4): Escalated for review.
- Auto (5): Automated system run.
- Whitelist: Excluded from scheduler and automatically approved.

### Current Case Context:
{context}

### Response Guidelines:
- Be concise and focus on what actions are needed next.
- If context is missing, suggest what the user should complete in the UI.";
        }

        public async Task<string> GetIntelligentReplyAsync(string prompt, string context)
        {
            try
            {
                var requestBody = new
                {
                    model = _modelName,
                    prompt = prompt,
                    system = GetSystemPrompt(context),
                    stream = false
                };

                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/generate", content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(jsonResponse);
                    return doc.RootElement.GetProperty("response").GetString();
                }

                return "I'm having trouble reaching the local AI service. Please ensure Ollama is running.";
            }
            catch (Exception ex)
            {
                return $"AI integration error: {ex.Message}";
            }
        }

        public async IAsyncEnumerable<string> GetIntelligentReplyStreamAsync(string prompt, string context, List<ChatMessageDTO> history = null)
        {
            var sbPrompt = new StringBuilder();
            if (history != null)
            {
                foreach (var msg in history)
                {
                    sbPrompt.Append($"<|{msg.Role}|>\n{msg.Content}\n\n");
                }
            }
            sbPrompt.Append($"<|user|>\n{prompt}\n\n<|assistant|>\n");

            var requestBody = new
            {
                model = _modelName,
                prompt = sbPrompt.ToString(),
                system = GetSystemPrompt(context),
                stream = true
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, "api/generate") { Content = content };
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                using var doc = JsonDocument.Parse(line);
                if (doc.RootElement.TryGetProperty("response", out var part))
                {
                    yield return part.GetString();
                }
            }
        }

        public async IAsyncEnumerable<string> GetGeneralReplyStreamAsync(string prompt, string moduleContext, List<ChatMessageDTO> history = null)
        {
            string systemKnowledge = GetSystemManual();
            var sbPrompt = new StringBuilder();
            sbPrompt.Append($"User Context: Currently on {moduleContext}\nSystem Knowledge Base: {systemKnowledge}\n\n");

            if (history != null)
            {
                foreach (var msg in history)
                {
                    sbPrompt.Append($"<|{msg.Role}|>\n{msg.Content}\n\n");
                }
            }
            sbPrompt.Append($"<|user|>\n{prompt}\n\n<|assistant|>\n");

            var requestBody = new
            {
                model = _modelName,
                prompt = sbPrompt.ToString(),
                system = "You are a helpful system assistant for Lemon WatchDog. Use Markdown with sections and bolding.",
                stream = true
            };
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, "api/generate") { Content = content };
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                using var doc = JsonDocument.Parse(line);
                if (doc.RootElement.TryGetProperty("response", out var part))
                {
                    yield return part.GetString();
                }
            }
        }

        private string GetSystemManual()
        {
            return @"
Lemon WatchDog System Manual:

1. Dashboard:
- Overview of all case statuses.
- Case Statuses: Pending (0), Approved (2), Rejected (3), Senior Mgmt (4), Auto (5), Daily Scheduler (6).
- High Risk cases require immediate attention.

2. Screening & KYC:
- Modules for 'Individual' and 'Corporate' creation.
- Mandatory fields: Name, ID/Company Code, Nationality/Country, Address.
- Risk Scoring: Automatically calculated based on assessment versions. High scores trigger 'Pending' status.
- Hit Handling: If a match is found during search, the case status becomes 'Pending'.

3. Proliferation Finance (PF):
- Search against global sanction and watchlists (MongoDB).
- Findings: Potential hits must be reviewed.
- Escalation: Findings can be submitted to Senior Management for further investigation.
- Decisions: Whitelist (Approval bypass), Approved, Reject, Hold.

4. Reports:
- Case Reports: Detailed PDF summaries of process and findings.
- Screening Logs: Audit trail of all database searches and results.
- Exporting: Use the PDF export buttons on Case/Details pages.

5. Admin Management:
- User Management: Create and edit users.
- User Groups: Manage permissions by grouping users (e.g., Senior Management, Compliance).
- Client Rights: Assign which clients/banks a user group can manage.
- Security: Access is strictly controlled by ClientId and Group rights.
";
        }
    }
}
