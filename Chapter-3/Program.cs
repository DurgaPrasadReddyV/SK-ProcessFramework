using Chapter_3.Events;
using Chapter_3.Models;
using Chapter_3.Steps;
using Chapter_3.Steps.TestInputs;
using Chapter_3.Utilities;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Process.Tools;

namespace Chapter_3
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Create a google kernel 
            var kernelBuilder = Kernel.CreateBuilder()
            .AddGoogleAIGeminiChatCompletion(modelId: "gemini-2.5-flash", apiKey: "");

            Kernel kernel = kernelBuilder.Build();

            KernelProcess kernelProcessSuccess = SetupAccountOpeningProcess<UserInputSuccessfulInteractionStep>();
            await using var runningProcessSuccess = await kernelProcessSuccess.StartAsync(kernel, new KernelProcessEvent() { Id = AccountOpeningEvents.StartProcess, Data = null });

            KernelProcess kernelProcessCreditFailure = SetupAccountOpeningProcess<UserInputCreditScoreFailureInteractionStep>();
            await using var runningProcessCreditFailure = await kernelProcessCreditFailure.StartAsync(kernel, new KernelProcessEvent() { Id = AccountOpeningEvents.StartProcess, Data = null });

            KernelProcess kernelProcessFraudFailure = SetupAccountOpeningProcess<UserInputFraudFailureInteractionStep>();
            await using var runningProcessFraudFailure = await kernelProcessFraudFailure.StartAsync(kernel, new KernelProcessEvent() { Id = AccountOpeningEvents.StartProcess, Data = null });
        }

        private static KernelProcess SetupAccountOpeningProcess<TUserInputStep>() where TUserInputStep : ScriptedUserInputStep
        {
            ProcessBuilder process = new("AccountOpeningProcess");
            var newCustomerFormStep = process.AddStepFromType<CompleteNewCustomerFormStep>();
            var userInputStep = process.AddStepFromType<TUserInputStep>();
            var displayAssistantMessageStep = process.AddStepFromType<DisplayAssistantMessageStep>();
            var customerCreditCheckStep = process.AddStepFromType<CreditScoreCheckStep>();
            var fraudDetectionCheckStep = process.AddStepFromType<FraudDetectionStep>();
            var mailServiceStep = process.AddStepFromType<MailServiceStep>();
            var coreSystemRecordCreationStep = process.AddStepFromType<NewAccountStep>();
            var marketingRecordCreationStep = process.AddStepFromType<NewMarketingEntryStep>();
            var crmRecordStep = process.AddStepFromType<CRMRecordCreationStep>();
            var welcomePacketStep = process.AddStepFromType<WelcomePacketStep>();

            process.OnInputEvent(AccountOpeningEvents.StartProcess)
                .SendEventTo(new ProcessFunctionTargetBuilder(newCustomerFormStep, CompleteNewCustomerFormStep.ProcessStepFunctions.NewAccountWelcome));

            // When the welcome message is generated, send message to displayAssistantMessageStep
            newCustomerFormStep
                .OnEvent(AccountOpeningEvents.NewCustomerFormWelcomeMessageComplete)
                .SendEventTo(new ProcessFunctionTargetBuilder(displayAssistantMessageStep, DisplayAssistantMessageStep.ProcessStepFunctions.DisplayAssistantMessage));

            // When the userInput step emits a user input event, send it to the newCustomerForm step
            // Function names are necessary when the step has multiple public functions like CompleteNewCustomerFormStep: NewAccountWelcome and NewAccountProcessUserInfo
            userInputStep
                .OnEvent(CommonEvents.UserInputReceived)
                .SendEventTo(new ProcessFunctionTargetBuilder(newCustomerFormStep, CompleteNewCustomerFormStep.ProcessStepFunctions.NewAccountProcessUserInfo, "userMessage"));

            userInputStep
                .OnEvent(CommonEvents.Exit)
                .StopProcess();

            // When the newCustomerForm step emits needs more details, send message to displayAssistantMessage step
            newCustomerFormStep
                .OnEvent(AccountOpeningEvents.NewCustomerFormNeedsMoreDetails)
                .SendEventTo(new ProcessFunctionTargetBuilder(displayAssistantMessageStep, DisplayAssistantMessageStep.ProcessStepFunctions.DisplayAssistantMessage));

            // After any assistant message is displayed, user input is expected to the next step is the userInputStep
            displayAssistantMessageStep
                .OnEvent(CommonEvents.AssistantResponseGenerated)
                .SendEventTo(new ProcessFunctionTargetBuilder(userInputStep, ScriptedUserInputStep.ProcessStepFunctions.GetUserInput));

            // When the newCustomerForm is completed...
            newCustomerFormStep
                .OnEvent(AccountOpeningEvents.NewCustomerFormCompleted)
                // The information gets passed to the core system record creation step
                .SendEventTo(new ProcessFunctionTargetBuilder(customerCreditCheckStep, functionName: CreditScoreCheckStep.ProcessStepFunctions.DetermineCreditScore, parameterName: "customerDetails"))
                // The information gets passed to the fraud detection step for validation
                .SendEventTo(new ProcessFunctionTargetBuilder(fraudDetectionCheckStep, functionName: FraudDetectionStep.ProcessStepFunctions.FraudDetectionCheck, parameterName: "customerDetails"))
                // The information gets passed to the core system record creation step
                .SendEventTo(new ProcessFunctionTargetBuilder(coreSystemRecordCreationStep, functionName: NewAccountStep.ProcessStepFunctions.CreateNewAccount, parameterName: "customerDetails"));

            // When the newCustomerForm is completed, the user interaction transcript with the user is passed to the core system record creation step
            newCustomerFormStep
                .OnEvent(AccountOpeningEvents.CustomerInteractionTranscriptReady)
                .SendEventTo(new ProcessFunctionTargetBuilder(coreSystemRecordCreationStep, functionName: NewAccountStep.ProcessStepFunctions.CreateNewAccount, parameterName: "interactionTranscript"));

            // When the creditScoreCheck step results in Rejection, the information gets to the mailService step to notify the user about the state of the application and the reasons
            customerCreditCheckStep
                .OnEvent(AccountOpeningEvents.CreditScoreCheckRejected)
                .SendEventTo(new ProcessFunctionTargetBuilder(mailServiceStep, functionName: MailServiceStep.ProcessStepFunctions.SendMailToUserWithDetails, parameterName: "message"));

            // When the creditScoreCheck step results in Approval, the information gets to the fraudDetection step to kickstart this step
            customerCreditCheckStep
                .OnEvent(AccountOpeningEvents.CreditScoreCheckApproved)
                .SendEventTo(new ProcessFunctionTargetBuilder(fraudDetectionCheckStep, functionName: FraudDetectionStep.ProcessStepFunctions.FraudDetectionCheck, parameterName: "previousCheckSucceeded"));

            // When the fraudDetectionCheck step fails, the information gets to the mailService step to notify the user about the state of the application and the reasons
            fraudDetectionCheckStep
                .OnEvent(AccountOpeningEvents.FraudDetectionCheckFailed)
                .SendEventTo(new ProcessFunctionTargetBuilder(mailServiceStep, functionName: MailServiceStep.ProcessStepFunctions.SendMailToUserWithDetails, parameterName: "message"));

            // When the fraudDetectionCheck step passes, the information gets to core system record creation step to kickstart this step
            fraudDetectionCheckStep
                .OnEvent(AccountOpeningEvents.FraudDetectionCheckPassed)
                .SendEventTo(new ProcessFunctionTargetBuilder(coreSystemRecordCreationStep, functionName: NewAccountStep.ProcessStepFunctions.CreateNewAccount, parameterName: "previousCheckSucceeded"));

            // When the coreSystemRecordCreation step successfully creates a new accountId, it will trigger the creation of a new marketing entry through the marketingRecordCreation step
            coreSystemRecordCreationStep
                .OnEvent(AccountOpeningEvents.NewMarketingRecordInfoReady)
                .SendEventTo(new ProcessFunctionTargetBuilder(marketingRecordCreationStep, functionName: NewMarketingEntryStep.ProcessStepFunctions.CreateNewMarketingEntry, parameterName: "userDetails"));

            // When the coreSystemRecordCreation step successfully creates a new accountId, it will trigger the creation of a new CRM entry through the crmRecord step
            coreSystemRecordCreationStep
                .OnEvent(AccountOpeningEvents.CRMRecordInfoReady)
                .SendEventTo(new ProcessFunctionTargetBuilder(crmRecordStep, functionName: CRMRecordCreationStep.ProcessStepFunctions.CreateCRMEntry, parameterName: "userInteractionDetails"));

            // ParameterName is necessary when the step has multiple input arguments like welcomePacketStep.CreateWelcomePacketAsync
            // When the coreSystemRecordCreation step successfully creates a new accountId, it will pass the account information details to the welcomePacket step
            coreSystemRecordCreationStep
                .OnEvent(AccountOpeningEvents.NewAccountDetailsReady)
                .SendEventTo(new ProcessFunctionTargetBuilder(welcomePacketStep, parameterName: "accountDetails"));

            // When the marketingRecordCreation step successfully creates a new marketing entry, it will notify the welcomePacket step it is ready
            marketingRecordCreationStep
                .OnEvent(AccountOpeningEvents.NewMarketingEntryCreated)
                .SendEventTo(new ProcessFunctionTargetBuilder(welcomePacketStep, parameterName: "marketingEntryCreated"));

            // When the crmRecord step successfully creates a new CRM entry, it will notify the welcomePacket step it is ready
            crmRecordStep
                .OnEvent(AccountOpeningEvents.CRMRecordInfoEntryCreated)
                .SendEventTo(new ProcessFunctionTargetBuilder(welcomePacketStep, parameterName: "crmRecordCreated"));

            // After crmRecord and marketing gets created, a welcome packet is created to then send information to the user with the mailService step
            welcomePacketStep
                .OnEvent(AccountOpeningEvents.WelcomePacketCreated)
                .SendEventTo(new ProcessFunctionTargetBuilder(mailServiceStep, functionName: MailServiceStep.ProcessStepFunctions.SendMailToUserWithDetails, parameterName: "message"));

            // All possible paths end up with the user being notified about the account creation decision throw the mailServiceStep completion
            mailServiceStep
                .OnEvent(AccountOpeningEvents.MailServiceSent)
                .StopProcess();

            KernelProcess kernelProcess = process.Build();

            return kernelProcess;
        }
    }
}
