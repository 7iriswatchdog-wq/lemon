using AML.Core.ServiceContract.AmlTracker;
using AML.DTO.DTO.AI;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AML.Web.Services
{
    public class ChatHistoryService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAmlTrackerService _trackerService;
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public ChatHistoryService(IMemoryCache memoryCache, IHttpContextAccessor httpContextAccessor, IAmlTrackerService trackerService)
        {
            _memoryCache = memoryCache;
            _httpContextAccessor = httpContextAccessor;
            _trackerService = trackerService;
        }

        private string GetSessionId() => _httpContextAccessor.HttpContext?.Session.Id ?? "anonymous";
        private string GetUserId() => _httpContextAccessor.HttpContext?.Session.GetString("SessUserId") ?? "0";
        private string GetClientId() => _httpContextAccessor.HttpContext?.Session.GetString("SessClientId") ?? "0";

        private string GetChatCacheKey(string chatType, string caseId = null)
        {
            string userId = GetUserId();
            string sessionId = GetSessionId();
            return string.IsNullOrEmpty(caseId)
                ? $"chat_{userId}_{sessionId}_{chatType}"
                : $"chat_{userId}_{sessionId}_{chatType}_{caseId}";
        }

        public List<ChatMessageDTO> GetChatHistory(string chatType, string caseId = null)
        {
            string cacheKey = GetChatCacheKey(chatType, caseId);
            _memoryCache.TryGetValue(cacheKey, out List<ChatMessageDTO> history);
            return history ?? new List<ChatMessageDTO>();
        }

        public void SaveChatHistory(string chatType, List<ChatMessageDTO> history, string caseId = null)
        {
            string cacheKey = GetChatCacheKey(chatType, caseId);

            // Persist new messages to DB before replacing the cache entry.
            try
            {
                int previousCount = _memoryCache.TryGetValue(cacheKey, out List<ChatMessageDTO> prev) && prev != null
                    ? prev.Count : 0;
                if (history != null && history.Count > previousCount)
                {
                    int.TryParse(GetUserId(), out var uid);
                    int.TryParse(GetClientId(), out var cid);
                    int? caseIdInt = null;
                    if (!string.IsNullOrWhiteSpace(caseId) && int.TryParse(caseId, out var cidp)) caseIdInt = cidp;

                    var sessionKey = $"{GetSessionId()}|{chatType}|{caseId ?? ""}";
                    for (int i = previousCount; i < history.Count; i++)
                    {
                        var m = history[i];
                        if (m == null) continue;
                        _trackerService.PersistChatMessage(
                            clientId: cid,
                            userId: uid,
                            sessionId: sessionKey,
                            caseId: caseIdInt,
                            role: m.Role ?? "user",
                            messageText: m.Content ?? "",
                            model: null,
                            promptTokens: null,
                            completionTokens: null,
                            latencyMs: null);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Chat history persistence failed (non-fatal; cache still updated)");
            }

            // #24 — register this key in the user's catalog for full cleanup
            string catalogKey = $"chat_catalog_{GetUserId()}_{GetSessionId()}";
            if (!_memoryCache.TryGetValue(catalogKey, out List<string> catalog) || catalog == null)
                catalog = new List<string>();
            if (!catalog.Contains(cacheKey)) catalog.Add(cacheKey);
            _memoryCache.Set(catalogKey, catalog, TimeSpan.FromHours(2));

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                .SetSlidingExpiration(TimeSpan.FromMinutes(30));
            _memoryCache.Set(cacheKey, history, cacheOptions);
        }

        public void ClearChatHistory(string chatType, string caseId = null)
        {
            string cacheKey = GetChatCacheKey(chatType, caseId);
            _memoryCache.Remove(cacheKey);
        }

        public void ClearAllUserChatHistory()
        {
            string userId = GetUserId();
            string sessionId = GetSessionId();

            // #24 — clear all chat types including case-specific keys by tracking them explicitly.
            // The cache does not support prefix-based removal, so we store known keys in a catalog.
            string catalogKey = $"chat_catalog_{userId}_{sessionId}";
            if (_memoryCache.TryGetValue(catalogKey, out List<string> keyCatalog) && keyCatalog != null)
            {
                foreach (var key in keyCatalog)
                    _memoryCache.Remove(key);
            }

            // Also remove well-known generic keys for safety
            foreach (var chatType in new[] { "case", "general", "dashboard" })
                _memoryCache.Remove($"chat_{userId}_{sessionId}_{chatType}");

            _memoryCache.Remove(catalogKey);
        }

        public async Task SaveChatHistoryAsync(string chatType, List<ChatMessageDTO> history, string caseId = null)
        {
            await Task.Run(() => SaveChatHistory(chatType, history, caseId));
        }
    }
}
