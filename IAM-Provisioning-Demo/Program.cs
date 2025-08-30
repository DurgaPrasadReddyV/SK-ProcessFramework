using IAM_Provisioning_Demo.Plugins;
using IAM_Provisioning_Demo.Steps;
using Microsoft.SemanticKernel;

namespace IAM_Provisioning_Demo
{
    public class Program
    {
        static void Main(string[] args)
        {
            // ===== Kernel set-up =====
            var kb = Kernel.CreateBuilder();
            // If you want model-driven function-calling/planning:
            // kb.AddAzureOpenAIChatCompletion("<deploymentId>", "<endpoint>", "<apiKey>");
            var kernel = kb.Build();

            // Register plugins (exposed to SK as callable functions) — see Plugins docs. :contentReference[oaicite:7]{index=7}
            kernel.Plugins.AddFromObject(new RequestIntakePlugin(), "RequestIntake");
            kernel.Plugins.AddFromObject(new ApprovalPlugin(), "Approvals");
            kernel.Plugins.AddFromObject(new NotificationPlugin(), "Notify");
            kernel.Plugins.AddFromObject(new AuditPlugin(), "Audit");
            kernel.Plugins.AddFromObject(new ProvisioningPlugin(), "Provision");

            // ===== Build the Process (Process Framework) — builder, steps, event routing. :contentReference[oaicite:8]{index=8}
            var processBuilder = new ProcessBuilder("IAM-Process");

            var intake = processBuilder.AddStepFromType<IntakeStep>();
            var route = processBuilder.AddStepFromType<ApprovalRoutingStep>();
            var collect = processBuilder.AddStepFromType<ApprovalCollectionStep>();
            var provision = processBuilder.AddStepFromType<ProvisioningStep>();
            var complete = processBuilder.AddStepFromType<CompletionStep>();

            processBuilder.OnInputEvent("Start").SendEventTo(new(intake,functionName: nameof(IntakeStep.Intake)));

            var iamProcess = processBuilder.Build();
        }
    }
}
