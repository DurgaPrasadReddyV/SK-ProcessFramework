using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Process.Runtime;
using Microsoft.SemanticKernel.Process.Tools;
using Temp.Models;
using Temp.Steps;
using Temp.Steps.Events;
using Temp.Steps.Functions;
using Temp.Utilities;

namespace Temp
{
    public class Program
    {
        public static List<ServiceAccountRequest> ServiceAccountRequests = new();
        public static List<UserAccountRequest> UserAccountRequests = new();
        public static KernelProcess? _kernelProcess;
        public static Kernel? _kernel;

        static async Task Main(string[] args)
        {

            // Create a google kernel 
            var kernelBuilder = Kernel.CreateBuilder()
            .AddGoogleAIGeminiChatCompletion(modelId: "gemini-2.5-flash", apiKey: "");
            _kernel = kernelBuilder.Build();

            ProcessBuilder process = new("ActiveDirectoryProvisioning");
            var welcomeStep = process.AddStepFromType<WelcomeStep>();
            var requestTypeSelectionUserInputStep = process.AddStepFromType<RequestTypeSelectionUserInputStep>();
            var displayRequestTypeSelectionAssistantMessageStep = process.AddStepFromType<DisplayRequestTypeSelectionAssistantMessageStep>();
            var requestIntakeStep = process.AddStepFromType<RequestIntakeStep>();
            var requestIntakeUserInputStep = process.AddStepFromType<RequestIntakeUserInputStep>();
            var displayRequestIntakeAssistantMessageStep = process.AddStepFromType<DisplayRequestIntakeAssistantMessageStep>();
            var approvalsGenerationStep = process.AddStepFromType<ApprovalsGenerationStep>();
            var mailServiceStep = process.AddStepFromType<MailServiceStep>();
            var approvalCollectionStep = process.AddStepFromType<ApprovalCollectionStep>();
            var provisioningStep = process.AddStepFromType<ProvisioningStep>();
            var completionStep = process.AddStepFromType<CompletionStep>();

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

            requestIntakeStep
               .OnEvent(RequestIntakeEvents.UserAccountRequestFormNeedsMoreDetails)
               .SendEventTo(new ProcessFunctionTargetBuilder(displayRequestIntakeAssistantMessageStep, DisplayAssistantMessageFunctions.ShowOnConsole));

            requestIntakeStep
               .OnEvent(RequestIntakeEvents.ServiceAccountRequestFormComplete)
               .SendEventTo(new ProcessFunctionTargetBuilder(approvalsGenerationStep, ApprovalsGenerationFunctions.GenerateServiceAccountApprovals, "request"));

            requestIntakeStep
               .OnEvent(RequestIntakeEvents.UserAccountRequestFormComplete)
               .SendEventTo(new ProcessFunctionTargetBuilder(approvalsGenerationStep, ApprovalsGenerationFunctions.GenerateUserAccountApprovals, "request"));

            approvalsGenerationStep
                .OnEvent(ApprovalsGenerationEvents.ServiceAccountApprovalsGenerated)
                .SendEventTo(new ProcessFunctionTargetBuilder(mailServiceStep, MailServiceFunctions.SendApprovalsEmail, "approvalSpec"))
                .SendEventTo(new ProcessFunctionTargetBuilder(approvalCollectionStep, ApprovalCollectionFunctions.InitializeApprovalsCollection, "approvalSpec"));

            approvalsGenerationStep
                .OnEvent(ApprovalsGenerationEvents.UserAccountApprovalsGenerated)
                .SendEventTo(new ProcessFunctionTargetBuilder(mailServiceStep, MailServiceFunctions.SendApprovalsEmail, "approvalSpec"))
                .SendEventTo(new ProcessFunctionTargetBuilder(approvalCollectionStep, ApprovalCollectionFunctions.InitializeApprovalsCollection, "approvalSpec"));

            process.OnInputEvent(ApprovalCollectionEvents.ApprovalReceived)
                .SendEventTo(new ProcessFunctionTargetBuilder(approvalCollectionStep, ApprovalCollectionFunctions.RecordApprovals, "approval"));

            approvalCollectionStep
                .OnEvent(ApprovalCollectionEvents.ApprovalRecorded)
                .SendEventTo(new ProcessFunctionTargetBuilder(approvalCollectionStep, ApprovalCollectionFunctions.CheckApprovalsCompletion));

            approvalCollectionStep
                .OnEvent(ApprovalCollectionEvents.AllApprovalsReceived)
                .SendEventTo(new ProcessFunctionTargetBuilder(approvalCollectionStep, ApprovalCollectionFunctions.FinalOutcome));

            approvalCollectionStep
                .OnEvent(ApprovalCollectionEvents.AllApprovalsApproved)
                .SendEventTo(new ProcessFunctionTargetBuilder(provisioningStep, ProvisioningFunctions.ProvisionEngineerReview, "requestId"));

            process.OnInputEvent(ProvisioningEvents.ProvisionEngineerReviewReceived)
                .SendEventTo(new ProcessFunctionTargetBuilder(provisioningStep, ProvisioningFunctions.ProcessProvisionEngineerReview, "provisionEngineerReview"));

            provisioningStep
                .OnEvent(ProvisioningEvents.ProvisionEngineerReviewApproved)
                .SendEventTo(new ProcessFunctionTargetBuilder(provisioningStep, ProvisioningFunctions.ProvisionResource, "requestId"));

            provisioningStep
                .OnEvent(ProvisioningEvents.ProcessCompleted)
                .SendEventTo(new ProcessFunctionTargetBuilder(completionStep, CompletionFunctions.Complete, "success"));

            // Build the process to get a handle that can be started
            _kernelProcess = process.Build();

            //// Generate a Mermaid diagram for the process and print it to the console
            //string mermaidGraph = _kernelProcess.ToMermaid();
            //Console.WriteLine($"=== Start - Mermaid Diagram for '{process.Name}' ===");
            //Console.WriteLine(mermaidGraph);
            //Console.WriteLine($"=== End - Mermaid Diagram for '{process.Name}' ===");

            //// Generate an image from the Mermaid diagram
            //string generatedImagePath = await MermaidRenderer.GenerateMermaidImageAsync(mermaidGraph, "ActiveDirectoryProvisioning.png");
            //Console.WriteLine($"Diagram generated at: {generatedImagePath}");

            // Start the process with an initial external event
            await using var runningProcess = await _kernelProcess.StartAsync(
                _kernel,
                new KernelProcessEvent()
                {
                    Id = WelcomeEvents.StartProcess,
                    Data = null
                });
        }

        public static async Task ProcessApprovalsAsync(Approval approval)
        {
            if (_kernelProcess is null || _kernel is null)
            {
                throw new InvalidOperationException("Kernel process is not initialized.");
            }

            // Simulate some delay for processing
            var random = new Random();
            await Task.Delay(random.Next(5000, 10000));

            approval.Approved = true; // Simulate approval
            approval.Timestamp = DateTime.UtcNow;
            await using var runningProcess = await _kernelProcess.StartAsync(
                _kernel,
                new KernelProcessEvent()
                {
                    Id = ApprovalCollectionEvents.ApprovalReceived,
                    Data = approval
                });
        }

        public static async Task ProcessProvisionEngineerReviewAsync(Guid requestId)
        {
            if (_kernelProcess is null || _kernel is null)
            {
                throw new InvalidOperationException("Kernel process is not initialized.");
            }

            // Simulate some delay for processing
            var random = new Random();
            await Task.Delay(random.Next(5000, 10000));

            await using var runningProcess = await _kernelProcess.StartAsync(
                _kernel,
                new KernelProcessEvent()
                {
                    Id = ProvisioningEvents.ProvisionEngineerReviewReceived,
                    Data = true
                });
        }
    }
}
