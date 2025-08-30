using Chapter_2.Events;
using Chapter_2.States;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter_2.Steps;

/// <summary>
/// A step that takes the user input from a previous step and generates a response from the chat completion service.
/// </summary>
public class ChatBotResponseStep : KernelProcessStep<ChatBotState>
{
    public static class ProcessFunctions
    {
        public const string GetChatResponse = nameof(GetChatResponse);
    }

    /// <summary>
    /// The public state object for the chat bot response step.
    /// </summary>
    public ChatBotState? _state;

    /// <summary>
    /// ActivateAsync is the place to initialize the state object for the step.
    /// </summary>
    /// <param name="state">An instance of <see cref="ChatBotState"/></param>
    /// <returns>A <see cref="ValueTask"/></returns>
    public override ValueTask ActivateAsync(KernelProcessStepState<ChatBotState> state)
    {
        _state = state.State;
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Generates a response from the chat completion service.
    /// </summary>
    /// <param name="context">The context for the current step and process. <see cref="KernelProcessStepContext"/></param>
    /// <param name="userMessage">The user message from a previous step.</param>
    /// <param name="_kernel">A <see cref="Kernel"/> instance.</param>
    /// <returns></returns>
    [KernelFunction(ProcessFunctions.GetChatResponse)]
    public async Task GetChatResponseAsync(KernelProcessStepContext context, string userMessage, Kernel _kernel)
    {
        _state!.ChatMessages.Add(new(AuthorRole.User, userMessage));
        IChatCompletionService chatService = _kernel.Services.GetRequiredService<IChatCompletionService>();
        ChatMessageContent response = await chatService.GetChatMessageContentAsync(_state.ChatMessages).ConfigureAwait(false);
        if (response == null)
        {
            throw new InvalidOperationException("Failed to get a response from the chat completion service.");
        }

        System.Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.WriteLine($"ASSISTANT: {response.Content}");
        System.Console.ResetColor();

        // Update state with the response
        _state.ChatMessages.Add(response);

        // emit event: assistantResponse
        await context.EmitEventAsync(new KernelProcessEvent { Id = ChatBotEvents.AssistantResponseGenerated, Data = response });
    }
}
