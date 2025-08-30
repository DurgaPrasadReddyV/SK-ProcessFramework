using Microsoft.SemanticKernel;
using Temp.Steps.Events;
using Temp.Steps.Functions;
using Temp.Steps.States;

namespace Temp.Steps;

public class RequestIntakeUserInputStep : KernelProcessStep
{
    [KernelFunction(UserInputFunctions.GetUserInput)]
    public virtual async ValueTask GetUserInputAsync(KernelProcessStepContext context)
    {
        Console.Write("USER: ");
        var userMessage = Console.ReadLine();
        // Emit the user input
        if (userMessage?.Equals("Exit", StringComparison.InvariantCultureIgnoreCase) ?? false)
        {
            await context.EmitEventAsync(new() { Id = UserInputEvents.Exit });
            return;
        }

        await context.EmitEventAsync(new() { Id = UserInputEvents.UserInputReceived, Data = userMessage });
    }
}
