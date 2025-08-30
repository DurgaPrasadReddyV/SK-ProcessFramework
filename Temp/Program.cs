using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Process.Runtime;
using Temp.Steps;
using Temp.Steps.Events;
using Temp.Steps.Functions;

namespace Temp
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            // Create a google kernel 
            var kernelBuilder = Kernel.CreateBuilder()
            .AddGoogleAIGeminiChatCompletion(modelId: "gemini-2.5-flash", apiKey: "");
            Kernel kernel = kernelBuilder.Build();

            ProcessBuilder process = new("ActiveDirectoryProvisioning");
            var welcomeStep = process.AddStepFromType<WelcomeStep>();
            var requestTypeSelectionUserInputStep = process.AddStepFromType<RequestTypeSelectionUserInputStep>();
            var displayRequestTypeSelectionAssistantMessageStep = process.AddStepFromType<DisplayRequestTypeSelectionAssistantMessageStep>();
            var requestIntakeStep = process.AddStepFromType<RequestIntakeStep>();
            var requestIntakeUserInputStep = process.AddStepFromType<RequestIntakeUserInputStep>();
            var displayRequestIntakeAssistantMessageStep = process.AddStepFromType<DisplayRequestIntakeAssistantMessageStep>();

            process.OnInputEvent(WelcomeEvents.StartProcess)
                .SendEventTo(new ProcessFunctionTargetBuilder(welcomeStep, WelcomeFunctions.Greetings));

            welcomeStep
                .OnEvent(WelcomeEvents.WelcomeMessageDisplayComplete)
                .SendEventTo(new ProcessFunctionTargetBuilder(displayRequestTypeSelectionAssistantMessageStep, DisplayAssistantMessageFunctions.ShowOnConsole));

            requestTypeSelectionUserInputStep
                .OnEvent(UserInputEvents.UserInputReceived)
                .SendEventTo(new ProcessFunctionTargetBuilder(welcomeStep, WelcomeFunctions.RequestTypeSelection, "userMessage"));

            requestTypeSelectionUserInputStep
                .OnEvent(UserInputEvents.Exit)
                .StopProcess();

            welcomeStep
                .OnEvent(WelcomeEvents.RequestTypeIsNotValid)
                .SendEventTo(new ProcessFunctionTargetBuilder(displayRequestTypeSelectionAssistantMessageStep, DisplayAssistantMessageFunctions.ShowOnConsole));

            displayRequestTypeSelectionAssistantMessageStep
                .OnEvent(DisplayAssistantMessageEvents.AssistantResponseGenerated)
                .SendEventTo(new ProcessFunctionTargetBuilder(requestTypeSelectionUserInputStep, UserInputFunctions.GetUserInput));

            welcomeStep
                .OnEvent(WelcomeEvents.RequestTypeSelectionComplete)
                .SendEventTo(new ProcessFunctionTargetBuilder(requestIntakeStep, RequestIntakeFunctions.RequestTypeCallout, "requestType"));

            requestIntakeStep
                .OnEvent(RequestIntakeEvents.RequestTypeCalloutComplete)
                .SendEventTo(new ProcessFunctionTargetBuilder(displayRequestIntakeAssistantMessageStep, DisplayAssistantMessageFunctions.ShowOnConsole));

            displayRequestIntakeAssistantMessageStep
                .OnEvent(DisplayAssistantMessageEvents.AssistantResponseGenerated)
                .SendEventTo(new ProcessFunctionTargetBuilder(requestIntakeUserInputStep, UserInputFunctions.GetUserInput));

            requestIntakeUserInputStep
                .OnEvent(UserInputEvents.UserInputReceived)
                .SendEventTo(new ProcessFunctionTargetBuilder(requestIntakeStep, RequestIntakeFunctions.CompleteRequestForm, "userMessage"));

            requestIntakeUserInputStep
                .OnEvent(UserInputEvents.Exit)
                .StopProcess();

            requestIntakeStep
               .OnEvent(RequestIntakeEvents.ServiceAccountRequestFormNeedsMoreDetails)
               .SendEventTo(new ProcessFunctionTargetBuilder(displayRequestIntakeAssistantMessageStep, DisplayAssistantMessageFunctions.ShowOnConsole));


            // Build the process to get a handle that can be started
            KernelProcess kernelProcess = process.Build();

            // Start the process with an initial external event
            await using var runningProcess = await kernelProcess.StartAsync(
                kernel,
                    new KernelProcessEvent()
                    {
                        Id = WelcomeEvents.StartProcess,
                        Data = null
                    });
        }
    }
}
