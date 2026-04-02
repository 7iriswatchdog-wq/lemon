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
3. **RICH FORMATTING**: Use **standard Markdown**. Use `### Header` for sections, and `**bold**` for key terms. When listing recommendations, steps, or multiple items, you MUST format them as a bulleted list (`- item`) or numbered list (`1. item`), rather than individual paragraphs. **NEVER** use long strings of dashes (e.g., `-------`) as separators.
4. **SPACING**: You MUST use **double newlines** (`\n\n`) between paragraphs. When creating lists, ensure a newline separates the list from the preceding paragraph.
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
- **BILINGUAL EXPERTISE**: If you encounter Arabic snippets from UAE Cabinet Decision 156 (Proliferation Finance), provide a professional English summary of the finding.
- **ACTIONABLE NEXT STEPS**: For any 'Potential Match' or high-risk finding, advise the user to escalate the case to **Senior Management (Status 4)** for final review using the 'Move to Senior Management' button in the UI.
- Be concise and focus on what actions are needed next.
- If context is missing, suggest what the user should complete in the UI.
- Always prioritize the provided [SYSTEM KNOWLEDGE BASE] for procedural rules.";
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

        public async IAsyncEnumerable<string> GetIntelligentReplyStreamAsync(string prompt, string context, List<ChatMessageDTO> history = null, bool isNewChat = false)
        {
            if (isNewChat)
            {
                string title = await GenerateTitleAsync(prompt);
                yield return $"[TITLE]: {title}";
            }

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

        public async IAsyncEnumerable<string> GetGeneralReplyStreamAsync(string prompt, string moduleContext, List<ChatMessageDTO> history = null, bool isNewChat = false)
        {
            if (isNewChat)
            {
                string title = await GenerateTitleAsync(prompt);
                yield return $"[TITLE]: {title}";
            }

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
                system = "You are a professional AML/KYC System Expert for 'Lemon WatchDog'. Answer all questions using only the provided User Context and System Knowledge Base. For Arabic PF hits (Cabinet Decision 156), provide an English translation and summary. Advise Status 4 (Senior Management) escalation for all potential hits. Use Markdown with clear sections, bold terms, and bulleted lists. NO technical jargon or status codes.",
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

        private async Task<string> GenerateTitleAsync(string prompt)
        {
            try
            {
                var requestBody = new
                {
                    model = _modelName,
                    prompt = $"Generate a 4-5 word catchy title for this conversation based on this user question: '{prompt}'. Return ONLY the title, no quotes or intro.",
                    system = "You are a helpful assistant. Be concise and professional.",
                    stream = false
                };

                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/generate", content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(jsonResponse);
                    string rawTitle = doc.RootElement.GetProperty("response").GetString();
                    return rawTitle.Trim().Trim('"').Trim('.');
                }
            }
            catch { }
            return "New Conversation";
        }

        private string GetSystemManual()
        {
            return @"
[SYSTEM EXPERT HANDBOOK: LEMON WATCHDOG]

1. DASHBOARD & MONITORING:
- TRACKING: Monitor case statuses: Pending (0), Approved (2), Rejected (3), Senior Management (4), Auto (5), and Daily Scheduler (6).
- HIGH RISK: Match scores > 80 are flagged as High Risk.
- SCHEDULER: Background engine that automatically rescreens cases daily.

2. SCREENING & CASE CREATION:
- MANDATORY FIELDS (*): Full Name, Nationality, Gender, Date of Birth, ID Number, ID Type.
- INPUT OPTIONS: Manual entry, OCR image extraction, and Bulk Excel upload.
- LISTS: Cases are screened against PEP (Politically Exposed), SAN (Sanctions), UN, and UAE Local lists.

3. PROLIFERATION FINANCE (PF):
- LEGAL COMPLIANCE: Cabinet Decision No. 156 of 2025 regarding dual-use items.
- SEARCHING: Supports Chemical/HS Code search and intelligent PDF keyword searching in official legislation.
- HIT REVIEW: Decisions include 'No Match', 'Potential Match', or 'Confirmed Hit'.

4. ADMIN & SECURITY:
- USER GROUPS: Admin, Compliance/Reviewer, and View-Only.
- RIGHTS: Permissions are Add, Edit, Delete, or View per menu.
- CLIENT RIGHTS: Feature flags (e.g., OCR/PF) defined per Client ID.

5. REPORTS & AUDIT:
- AUDIT TRAIL: Every event is logged in the system's Audit Trail (ScreeningLogs table).
- EVIDENCE: Use 'Export' to generate a Case Process PDF for compliance records.
";
        }
    }
}
