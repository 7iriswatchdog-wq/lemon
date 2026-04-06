using AML.DTO.DTO.AI;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AML.Web.Services
{
    public class ChatHistoryService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChatHistoryService(IMemoryCache memoryCache, IHttpContextAccessor httpContextAccessor)
        {
            _memoryCache = memoryCache;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetSessionId()
        {
            return _httpContextAccessor.HttpContext?.Session.Id ?? "anonymous";
        }

        private string GetUserId()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("SessUserId") ?? "anonymous";
        }

        private string GetChatCacheKey(string chatType, string caseId = null)
        {
            string userId = GetUserId();
            string sessionId = GetSessionId();
            
            if (string.IsNullOrEmpty(caseId))
            {
                return $"chat_{userId}_{sessionId}_{chatType}";
            }
            else
            {
                return $"chat_{userId}_{sessionId}_{chatType}_{caseId}";
            }
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
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(1)) // Keep chat history for 1 hour
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
            
            // This is a simple approach - in production you might want a more sophisticated cache key management
            // For now, we'll clear common chat types
            var chatTypes = new[] { "case", "general", "dashboard" };
            
            foreach (var chatType in chatTypes)
            {
                string cacheKey = $"chat_{userId}_{sessionId}_{chatType}";
                _memoryCache.Remove(cacheKey);
            }
        }

        public async Task SaveChatHistoryAsync(string chatType, List<ChatMessageDTO> history, string caseId = null)
        {
            await Task.Run(() => SaveChatHistory(chatType, history, caseId));
        }
    }
}