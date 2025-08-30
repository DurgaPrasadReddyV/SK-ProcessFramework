using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Temp.Models;
using Temp.Steps.Events;
using Temp.Steps.Functions;
using Temp.Steps.States;

namespace Temp.Steps;

public sealed class WelcomeStep : KernelProcessStep<WelcomeState>
{
    internal string _welcomeMessage = """
    Welcome! I'm here to help you provision user accounts and access permissions within active directory.
    
    I can assist you with the following tasks:
    1. Creating service accounts
    2. Setting up user accounts  
    3. Managing group memberships
    4. Creating groups
    
    Type the number  (1-4) or the name (CreateServiceAccount/CreateUserAccount/ManageGroupMembership/CreateGroup).
    Please let me know which option you'd like to work with to get started. Type 'exit' to leave the process at any time.
    """;

    internal WelcomeState? _state;

    internal string _requestTypeSelectionSystemPrompt = """
        You are a helpful assistant designed to guide users through selecting active directory objects creation options. Your role is to help users choose the most appropriate option from the following menu:

        1. Creating service accounts
        2. Setting up user accounts
        3. Managing group memberships
        4. Creating and organizing groups

        <CURRENT_REQUEST_VALUE>
        {{current_request_value}}
        <CURRENT_REQUEST_VALUE>

        INSTRUCTIONS:
        - Always be friendly, patient, and clear in your responses
        - Ask clarifying questions to understand the user's specific needs
        - Provide brief explanations of what each option involves when needed
        - Guide users to the most appropriate choice based on their requirements
        - If a user is unsure, help them by asking about their goals or what they're trying to accomplish
        - Keep responses concise but informative
        - Once a user selects an option, confirm their choice and indicate you're ready to proceed

        EXAMPLE INTERACTIONS:
        - If user says "I need to add a new employee": Guide them toward option 2 (Setting up user accounts)
        - If user says "I need an account for our application": Guide them toward option 1 (Creating service accounts)
        - If user says "I want to give someone access to a folder": Guide them toward option 3 (Managing group memberships)
        - If user says "I'm not sure": Ask what they're trying to accomplish or who needs access to what

        Remember: Your goal is to help users quickly identify which option best fits their needs through friendly conversation and targeted questions.
        """;

    [KernelFunction(WelcomeFunctions.Greetings)]
    public async Task WelcomeMessageAsync(KernelProcessStepContext context, Kernel _kernel)
    {
        _state?.Conversation.Add(new ChatMessageContent { Role = AuthorRole.Assistant, Content = _welcomeMessage });
        await context.EmitEventAsync(new() { Id = WelcomeEvents.WelcomeMessageDisplayComplete, Data = _welcomeMessage });
    }

    [KernelFunction(WelcomeFunctions.RequestTypeSelection)]
    public async Task CompleteRequestTypeSelectionAsync(KernelProcessStepContext context, string userMessage, Kernel _kernel)
    {
        // Keeping track of all user interactions
        _state?.Conversation.Add(new ChatMessageContent { Role = AuthorRole.User, Content = userMessage });

        Kernel kernel = CreateNewRequestTypeKernel(_kernel);

        GeminiPromptExecutionSettings settings = new()
        {
            ToolCallBehavior = GeminiToolCallBehavior.AutoInvokeKernelFunctions,
            Temperature = 0.7,
            MaxTokens = 4096
        };

        ChatHistory chatHistory = new();
        chatHistory.AddSystemMessage(_requestTypeSelectionSystemPrompt
            .Replace("{{current_request_value}}", JsonSerializer.Serialize(_state?.RequestType, _jsonOptions)));
        chatHistory.AddRange(_state?.Conversation ?? new List<ChatMessageContent>());
        IChatCompletionService chatService = kernel.Services.GetRequiredService<IChatCompletionService>();
        ChatMessageContent response = await chatService.GetChatMessageContentAsync(chatHistory, settings, kernel).ConfigureAwait(false);
        var assistantResponse = "";

        if (response != null)
        {
            assistantResponse = response.Items[0].ToString();
            // Keeping track of all assistant interactions
            _state?.Conversation.Add(new ChatMessageContent { Role = AuthorRole.Assistant, Content = assistantResponse });
        }

        if (_state?.RequestType != null && _state.RequestType.IsValid())
        {
            Console.WriteLine($"[REQUEST_TYPE_SELECTION_COMPLETED]: {JsonSerializer.Serialize(_state?.RequestType, _jsonOptions)}");
            // Request type is gathered to proceed to the next step
            await context.EmitEventAsync(new() { Id = WelcomeEvents.RequestTypeSelectionComplete, Data = _state?.RequestType, Visibility = KernelProcessEventVisibility.Public });
            await context.EmitEventAsync(new() { Id = WelcomeEvents.RequestTypeCustomerInteractionTranscriptReady, Data = _state?.Conversation, Visibility = KernelProcessEventVisibility.Public });
            return;
        }

        // emit event: request type is not valid yet
        await context.EmitEventAsync(new() { Id = WelcomeEvents.RequestTypeIsNotValid, Data = assistantResponse });
    }

    public override ValueTask ActivateAsync(KernelProcessStepState<WelcomeState> state)
    {
        _state = state.State;
        return ValueTask.CompletedTask;
    }

    private Kernel CreateNewRequestTypeKernel(Kernel _baseKernel)
    {
        // Creating another kernel that only makes use private functions to get the request type from the user
        Kernel kernel = new(_baseKernel.Services);
        kernel.ImportPluginFromFunctions("FetchRequestType", [
            KernelFunctionFactory.CreateFromMethod(OnUserProvidedRequestType, functionName: nameof(OnUserProvidedRequestType)),
        ]);

        return kernel;
    }

    [Description("User provided details of request type value. " +
        "Allowed: 1/2/3/4 or CreateServiceAccount/CreateUserAccount/ManageGroupMembership/CreateGroup ")]
    private void OnUserProvidedRequestType(string requestType)
    {
        if (TryParseRequestType(requestType, out var eRequestType))
        {
            if (_state != null && _state.RequestType != null)
                _state.RequestType.Type = eRequestType;
        }
    }

    private static bool TryParseRequestType(string input, out ERequestType eRequestType)
    {
        // Try numeric first
        if (int.TryParse(input, out var num) && Enum.IsDefined(typeof(ERequestType), num))
        {
            eRequestType = (ERequestType)num;
            return true;
        }

        // Then try enum name (case-insensitive)
        if (Enum.TryParse(input, ignoreCase: true, out eRequestType))
            return true;

        eRequestType = ERequestType.Unknown;
        return false;
    }

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false) }
    };
}


