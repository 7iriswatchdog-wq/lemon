using AML.DTO.DTO.AI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AML.Core.ServiceContract.AI
{
    public interface IAIService
    {
        /// <summary>
        /// Generates an intelligent AI reply based on a user prompt and provided context.
        /// </summary>
        Task<string> GetIntelligentReplyAsync(string prompt, string context);

        /// <summary>
        /// Streams an intelligent AI reply word-by-word with conversation history.
        /// </summary>
        IAsyncEnumerable<string> GetIntelligentReplyStreamAsync(string prompt, string context, List<ChatMessageDTO> history = null);

        /// <summary>
        /// Streams a general AI reply about system knowledge with conversation history.
        /// </summary>
        IAsyncEnumerable<string> GetGeneralReplyStreamAsync(string prompt, string moduleContext, List<ChatMessageDTO> history = null);
    }
}
