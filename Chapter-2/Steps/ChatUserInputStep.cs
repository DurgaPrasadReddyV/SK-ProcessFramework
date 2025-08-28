using Chapter_2.Events;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter_2.Steps;

/// <summary>
/// A step that elicits user input.
/// </summary>
public class ChatUserInputStep : ScriptedUserInputStep
{
    public override void PopulateUserInputs(UserInputState state)
    {
        state.UserInputs.Add("Hello");
        state.UserInputs.Add("How tall is the tallest mountain?");
        state.UserInputs.Add("How low is the lowest valley?");
        state.UserInputs.Add("How wide is the widest river?");
        state.UserInputs.Add("exit");
        state.UserInputs.Add("This text will be ignored because exit process condition was already met at this point.");
    }

    public override async ValueTask GetUserInputAsync(KernelProcessStepContext context)
    {
        var userMessage = this.GetNextUserMessage();

        if (string.Equals(userMessage, "exit", StringComparison.OrdinalIgnoreCase))
        {
            // exit condition met, emitting exit event
            await context.EmitEventAsync(new() { Id = ChatBotEvents.Exit, Data = userMessage });
            return;
        }

        // emitting userInputReceived event
        await context.EmitEventAsync(new() { Id = CommonEvents.UserInputReceived, Data = userMessage });
    }
}
