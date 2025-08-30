using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Temp.Models;
using Temp.Steps.Events;
using Temp.Steps.Functions;
using Temp.Steps.States;

namespace Temp.Steps;

public class RequestIntakeStep : KernelProcessStep<RequestIntakeState>
{
    public RequestIntakeState? _state;

    public string _formCompletionSystemPrompt = """
        The goal is to fill up all the fields needed for a form.
        The user may provide information to fill up multiple fields of the form in one message.
        The user needs to fill up a form, all the fields of the form are necessary

        <CURRENT_FORM_STATE>
        {{current_form_state}}
        <CURRENT_FORM_STATE>

        GUIDANCE:
        - If there are missing details, give the user a useful message that will help fill up the remaining fields.
        - Your goal is to help guide the user to provide the missing details on the current form.
        - Encourage the user to provide the remainingdetails with examples if necessary.
        - Fields with value 'Unanswered' need to be answered by the user.
        - Format phone numbers and user ids correctly if the user does not provide the expected format.
        - If the user does not make use of parenthesis in the phone number, add them.
        - For date fields, confirm with the user first if the date format is not clear. Example 02/03 03/02 could be March 2nd or February 3rd.
        """;

    public string _calloutMessage = """
        You have selected {{request_type}}. I will help you fill up the necessary information needed to submit the request.
        """;

    [KernelFunction(RequestIntakeFunctions.RequestTypeCallout)]
    public async Task RequestTypeCalloutAsync(KernelProcessStepContext context, RequestType requestType)
    {
        if (_state != null)
        {
            _state.RequestType = requestType.Type;
            _state.Conversation.Add(new ChatMessageContent { Role = AuthorRole.Assistant, Content = _calloutMessage.Replace("{{request_type}}", requestType.Type.ToString()) });
        }
        await context.EmitEventAsync(new() { Id = RequestIntakeEvents.RequestTypeCalloutComplete, Data = _calloutMessage.Replace("{{request_type}}", requestType.Type.ToString()) });
    }

    [KernelFunction(RequestIntakeFunctions.CompleteRequestForm)]
    public virtual async ValueTask CompleteRequestFormAsync(KernelProcessStepContext context, string userMessage, Kernel kernel)
    {
        _state?.Conversation.Add(new ChatMessageContent { Role = AuthorRole.User, Content = userMessage });

        switch (_state?.RequestType)
        {
            case ERequestType.CreateServiceAccount:
                await ProcessServiceAccountRequestAsync(context, kernel);
                break;

            default:
                Console.WriteLine("This switch case should not be executed.");
                break;
        }
    }

    private async Task ProcessServiceAccountRequestAsync(KernelProcessStepContext context, Kernel _kernel)
    {
        Kernel kernel = CreateServiceAccountRequestFormKernel(_kernel);

        GeminiPromptExecutionSettings settings = new()
        {
            ToolCallBehavior = GeminiToolCallBehavior.AutoInvokeKernelFunctions,
            Temperature = 0.7,
            MaxTokens = 2048
        };

        ChatHistory chatHistory = new();
        chatHistory.AddSystemMessage(_formCompletionSystemPrompt
            .Replace("{{current_form_state}}", JsonSerializer.Serialize(_state?.ServiceAccountRequest?.CopyWithDefaultValues(), _jsonOptions)));
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

        if (_state?.ServiceAccountRequest != null && _state.ServiceAccountRequest.IsFormCompleted())
        {
            Console.WriteLine($"[SERVICE_ACCOUNT_REQUEST_FORM_COMPLETED]: {JsonSerializer.Serialize(_state?.ServiceAccountRequest)}");
            // All user information is gathered to proceed to the next step
            await context.EmitEventAsync(new() { Id = RequestIntakeEvents.ServiceAccountRequestFormComplete, Data = _state?.ServiceAccountRequest, Visibility = KernelProcessEventVisibility.Public });
            await context.EmitEventAsync(new() { Id = RequestIntakeEvents.ServiceAccountRequestCustomerInteractionTranscriptReady, Data = _state?.Conversation, Visibility = KernelProcessEventVisibility.Public });
            return;
        }

        await context.EmitEventAsync(new() { Id = RequestIntakeEvents.ServiceAccountRequestFormNeedsMoreDetails, Data = assistantResponse });
    }

    private Kernel CreateServiceAccountRequestFormKernel(Kernel _baseKernel)
    {
        // Creating another kernel that only makes use private functions to fill up the new customer form
        Kernel kernel = new(_baseKernel.Services);
        kernel.ImportPluginFromFunctions("FillServiceAccountRequestForm", [
        KernelFunctionFactory.CreateFromMethod(OnUserProvidedAccountNameForServiceAccount, functionName: nameof(OnUserProvidedAccountNameForServiceAccount)),
        ]);

        return kernel;
    }

    [Description("User provided details of account name")]
    private void OnUserProvidedAccountNameForServiceAccount(string accountName)
    {
        if (!string.IsNullOrEmpty(accountName) && _state != null && _state.ServiceAccountRequest != null)
        {
            _state.ServiceAccountRequest.AccountName = accountName;
        }
    }

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };

    public override ValueTask ActivateAsync(KernelProcessStepState<RequestIntakeState> state)
    {
        _state = state.State;
        return ValueTask.CompletedTask;
    }
}
