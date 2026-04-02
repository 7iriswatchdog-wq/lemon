using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AML.Core.ServiceContract.AI;
using Microsoft.Extensions.Configuration;

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

        public async IAsyncEnumerable<string> GetIntelligentReplyStreamAsync(string prompt, string context)
        {
            var requestBody = new
            {
                model = _modelName,
                prompt = prompt,
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

                if (doc.RootElement.TryGetProperty("done", out var done) && done.GetBoolean())
                {
                    break;
                }
            }
        }
    }
}
