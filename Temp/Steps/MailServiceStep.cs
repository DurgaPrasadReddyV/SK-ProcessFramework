using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Services;
using Temp.Models;
using Temp.Steps.Events;
using Temp.Steps.Functions;

namespace Temp.Steps;

/// <summary>
/// Mock step that emulates Mail Service with a message for the user.
/// </summary>
public class MailServiceStep : KernelProcessStep
{
    [KernelFunction(MailServiceFunctions.SendSimpleMessageEmail)]
    public async Task SendSimpleMessageMailAsync(KernelProcessStepContext context, string message)
    {
        Console.WriteLine("======== MAIL SERVICE ======== ");
        Console.WriteLine(message);
        Console.WriteLine("============================== ");

        await context.EmitEventAsync(new() { Id = MailServiceEvents.SimpleMessageMailSent, Data = message });
    }

    [KernelFunction(MailServiceFunctions.SendApprovalsEmail)]
    public async Task SendApprovalsMailAsync(KernelProcessStepContext context, ApprovalSpec approvalSpec)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("======== MAIL SERVICE ======== ");

        Console.WriteLine($"Approval Request ID: {approvalSpec.RequestId}");
        if (approvalSpec.RequiredApprovals != null)
        {
            foreach (var approval in approvalSpec.RequiredApprovals)
            {
                Console.WriteLine($"Approval ID: {approval.Id}");
                Console.WriteLine($"Approver: {approval.Approver}");
                Console.WriteLine($"Role: {approval.Role}");
            }
        }
        else
        {
            Console.WriteLine("No approvals found.");
        }

        Console.WriteLine("============================== ");
        Console.ResetColor();

        await context.EmitEventAsync(new() { Id = MailServiceEvents.ApprovalsMailSent, Data = approvalSpec });
    }
}
