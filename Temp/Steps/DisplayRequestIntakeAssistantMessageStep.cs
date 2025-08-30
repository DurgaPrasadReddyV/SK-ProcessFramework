using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temp.Steps.Events;
using Temp.Steps.Functions;

namespace Temp.Steps;

public class DisplayRequestIntakeAssistantMessageStep : KernelProcessStep
{
    [KernelFunction(DisplayAssistantMessageFunctions.ShowOnConsole)]
    public async ValueTask DisplayAssistantMessageAsync(KernelProcessStepContext context, string assistantMessage)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"ASSISTANT: {assistantMessage}\n");
        Console.ResetColor();

        // Emit the assistantMessageGenerated
        await context.EmitEventAsync(new() { Id = DisplayAssistantMessageEvents.AssistantResponseGenerated, Data = assistantMessage });
    }
}
