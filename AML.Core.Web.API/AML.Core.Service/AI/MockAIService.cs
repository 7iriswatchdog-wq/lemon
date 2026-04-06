using AML.Core.ServiceContract.AI;
using AML.DTO.DTO.AI;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AML.Core.Service.AI
{
    public class MockAIService : BaseService, IAIService
    {
        public MockAIService(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<string> GetIntelligentReplyAsync(string prompt, string context)
        {
            // Custom logic to handle predefined questions
            bool hasCaseId = context.Contains("Case ID") || context.Contains("case ID") || 
                           prompt.Contains("Case ID") || prompt.Contains("case ID") || 
                           prompt.Contains("NAT") || prompt.Contains("nat");

            if (prompt.Contains("What is the status of the case?"))
            {
                if (hasCaseId)
                {
                    return "The case status is currently under review. Please check back later for updates.";
                }
                else
                {
                    return "Please provide the Case ID to check the status.";
                }
            }
            else if (prompt.Contains("What should I do next?"))
            {
                if (hasCaseId)
                {
                    return "You should gather additional information and submit it for further review.";
                }
                else
                {
                    return "Please provide the Case ID to get the next steps.";
                }
            }
            else if (prompt.Contains("What was the case history?"))
            {
                if (hasCaseId)
                {
                    return "The case history includes initial screening, risk assessment, and pending review by the compliance team.";
                }
                else
                {
                    return "Please provide the Case ID to check the history.";
                }
            }
            else
            {
                return "The chatbot is currently unavailable for custom queries. Please try again later or contact support.";
            }
        }

        public async IAsyncEnumerable<string> GetIntelligentReplyStreamAsync(string prompt, string context, List<ChatMessageDTO> history = null, bool isNewChat = false)
        {
            // Custom logic to handle predefined questions
            bool hasCaseId = context.Contains("Case ID") || context.Contains("case ID") || 
                           prompt.Contains("Case ID") || prompt.Contains("case ID") || 
                           prompt.Contains("NAT") || prompt.Contains("nat");

            if (prompt.Contains("What is the status of the case?"))
            {
                if (hasCaseId)
                {
                    yield return "The case status is currently under review. Please check back later for updates.";
                }
                else
                {
                    yield return "Please provide the Case ID to check the status.";
                }
            }
            else if (prompt.Contains("What should I do next?"))
            {
                if (hasCaseId)
                {
                    yield return "You should gather additional information and submit it for further review.";
                }
                else
                {
                    yield return "Please provide the Case ID to get the next steps.";
                }
            }
            else if (prompt.Contains("What was the case history?"))
            {
                if (hasCaseId)
                {
                    yield return "The case history includes initial screening, risk assessment, and pending review by the compliance team.";
                }
                else
                {
                    yield return "Please provide the Case ID to check the history.";
                }
            }
            else
            {
                yield return "The chatbot is currently unavailable for custom queries. Please try again later or contact support.";
            }
        }

        public async IAsyncEnumerable<string> GetGeneralReplyStreamAsync(string prompt, string moduleContext, List<ChatMessageDTO> history = null, bool isNewChat = false)
        {
            // Custom logic to handle predefined questions
            bool hasCaseId = moduleContext.Contains("Case ID") || moduleContext.Contains("case ID") || 
                           prompt.Contains("Case ID") || prompt.Contains("case ID") || 
                           prompt.Contains("NAT") || prompt.Contains("nat");

            if (prompt.Contains("What is the status of the case?"))
            {
                if (hasCaseId)
                {
                    yield return "The case status is currently under review. Please check back later for updates.";
                }
                else
                {
                    yield return "Please provide the Case ID to check the status.";
                }
            }
            else if (prompt.Contains("What should I do next?"))
            {
                if (hasCaseId)
                {
                    yield return "You should gather additional information and submit it for further review.";
                }
                else
                {
                    yield return "Please provide the Case ID to get the next steps.";
                }
            }
            else if (prompt.Contains("What was the case history?"))
            {
                if (hasCaseId)
                {
                    yield return "The case history includes initial screening, risk assessment, and pending review by the compliance team.";
                }
                else
                {
                    yield return "Please provide the Case ID to check the history.";
                }
            }
            else
            {
                yield return "The chatbot is currently unavailable for custom queries. Please try again later or contact support.";
            }
        }
    }
}
