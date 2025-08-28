using Chapter_3.Events;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter_3.Steps;

/// <summary>
/// Step used in the Processes Samples:
/// - Step_02_AccountOpening.cs
/// </summary>
public class DisplayAssistantMessageStep : KernelProcessStep
{
    public static class ProcessStepFunctions
    {
        public const string DisplayAssistantMessage = nameof(DisplayAssistantMessage);
    }

    [KernelFunction(ProcessStepFunctions.DisplayAssistantMessage)]
    public async ValueTask DisplayAssistantMessageAsync(KernelProcessStepContext context, string assistantMessage)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"ASSISTANT: {assistantMessage}\n");
        Console.ResetColor();

        // Emit the assistantMessageGenerated
        await context.EmitEventAsync(new() { Id = CommonEvents.AssistantResponseGenerated, Data = assistantMessage });
    }
}
