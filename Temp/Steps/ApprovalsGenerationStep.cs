using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using StarodubOleg.GPPG.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Temp.Models;
using Temp.Steps.Events;
using Temp.Steps.Functions;
using Temp.Steps.States;

namespace Temp.Steps
{
    public class ApprovalsGenerationStep : KernelProcessStep
    {
        public string _approvalsGenerationSystemPrompt = """
            You are an IAM request approval spec generator.
            Produce a STRICT JSON object matching response schema.
            
            Rules:
            - Use ONLY roles and selectors from the provided policy rules.
            - NEVER invent identities; do not output names.
            - The 'selector' must come verbatim from the policy (e.g., ""ManagerOf(TargetUser)"").
            - If a selector allows alternatives, choose the most specific available for the request
              (e.g., ""ResourceOwnerOf(TargetService|RequestType)"" -> prefer TargetService if present, else RequestType).
            - Output JSON only, no comments.
            """;

        [KernelFunction(ApprovalsGenerationFunctions.GenerateServiceAccountApprovals)]
        public virtual async ValueTask GenerateServiceAccountApprovalsAsync(KernelProcessStepContext context, ServiceAccountRequest request, Kernel _kernel)
        {
            var rule = PolicyMatrix.Rules.FirstOrDefault(r => r.RequestType.Equals(typeof(ServiceAccountRequest).Name, StringComparison.OrdinalIgnoreCase));
            if (rule is null) throw new NotSupportedException($"No policy rule found for RequestType='{typeof(ServiceAccountRequest)}'.");

            Kernel kernel = new(_kernel.Services);

            GeminiPromptExecutionSettings settings = new()
            {
                ResponseSchema = typeof(ApprovalSpecDraft),
                ResponseMimeType = "application/json",
                ToolCallBehavior = GeminiToolCallBehavior.AutoInvokeKernelFunctions,
                Temperature = 0,
                MaxTokens = 2048
            };

            ChatHistory chatHistory = new();
            chatHistory.AddSystemMessage(_approvalsGenerationSystemPrompt);

            var userMessage = BuildServiceAccountRequestPrompt(rule, request);
            chatHistory.Add(new ChatMessageContent { Role = AuthorRole.User, Content = userMessage });

            IChatCompletionService chatService = kernel.Services.GetRequiredService<IChatCompletionService>();
            ChatMessageContent response = await chatService.GetChatMessageContentAsync(chatHistory, settings, kernel).ConfigureAwait(false);
            var assistantResponse = "";

            if (response != null)
            {
                assistantResponse = response.Items[0].ToString();
            }

            ApprovalSpecDraft? draft;
            try
            {
                draft = JsonSerializer.Deserialize<ApprovalSpecDraft>(assistantResponse ?? string.Empty);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("ApprovalSpecDraft JSON invalid: " + ex.Message + "\nLLM Output:\n" + assistantResponse);
            }
            if (draft is null || draft.RequestId != request.Id || draft.DraftSteps is null || draft.DraftSteps.Count == 0)
                throw new InvalidOperationException("ApprovalSpecDraft missing required fields.");

            var approvalSpec = ApprovalSpecResolver.ResolveServiceAccountRequest(draft, request);
            await context.EmitEventAsync(new() { Id = ApprovalsGenerationEvents.ServiceAccountApprovalsGenerated, Data = approvalSpec, Visibility = KernelProcessEventVisibility.Public });
        }

        [KernelFunction(ApprovalsGenerationFunctions.GenerateUserAccountApprovals)]
        public virtual async ValueTask GenerateUserAccountApprovalsAsync(KernelProcessStepContext context, UserAccountRequest request, Kernel _kernel)
        {
            var rule = PolicyMatrix.Rules.FirstOrDefault(r => r.RequestType.Equals(typeof(UserAccountRequest).Name, StringComparison.OrdinalIgnoreCase));
            if (rule is null) throw new NotSupportedException($"No policy rule found for RequestType='{typeof(UserAccountRequest)}'.");

            Kernel kernel = new(_kernel.Services);

            GeminiPromptExecutionSettings settings = new()
            {
                ResponseSchema = typeof(ApprovalSpecDraft),
                ResponseMimeType = "application/json",
                ToolCallBehavior = GeminiToolCallBehavior.AutoInvokeKernelFunctions,
                Temperature = 0,
                MaxTokens = 2048
            };

            ChatHistory chatHistory = new();
            chatHistory.AddSystemMessage(_approvalsGenerationSystemPrompt);

            var userMessage = BuildUserAccountRequestPrompt(rule, request);
            chatHistory.Add(new ChatMessageContent { Role = AuthorRole.User, Content = userMessage });

            IChatCompletionService chatService = kernel.Services.GetRequiredService<IChatCompletionService>();
            ChatMessageContent response = await chatService.GetChatMessageContentAsync(chatHistory, settings, kernel).ConfigureAwait(false);
            var assistantResponse = "";

            if (response != null)
            {
                assistantResponse = response.Items[0].ToString();
            }

            ApprovalSpecDraft? draft;
            try
            {
                draft = JsonSerializer.Deserialize<ApprovalSpecDraft>(assistantResponse ?? string.Empty);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("ApprovalSpecDraft JSON invalid: " + ex.Message + "\nLLM Output:\n" + assistantResponse);
            }
            if (draft is null || draft.RequestId != request.Id || draft.DraftSteps is null || draft.DraftSteps.Count == 0)
                throw new InvalidOperationException("ApprovalSpecDraft missing required fields.");

            var approvalSpec = ApprovalSpecResolver.ResolveUserAccountRequest(draft, request);
            await context.EmitEventAsync(new() { Id = ApprovalsGenerationEvents.UserAccountApprovalsGenerated, Data = approvalSpec, Visibility = KernelProcessEventVisibility.Public });
        }

        static string BuildServiceAccountRequestPrompt(PolicyRule rule, ServiceAccountRequest req) =>
        $@"
            POLICY RULE: {JsonSerializer.Serialize(rule)}
            REQUEST: {JsonSerializer.Serialize(req)}
            ";

        static string BuildUserAccountRequestPrompt(PolicyRule rule, UserAccountRequest req) =>
            $@"
            POLICY RULE: {System.Text.Json.JsonSerializer.Serialize(rule)}
            REQUEST: {System.Text.Json.JsonSerializer.Serialize(req)}
            ";
    }
}
