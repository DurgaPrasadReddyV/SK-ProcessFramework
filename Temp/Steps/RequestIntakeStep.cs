using Microsoft.SemanticKernel;
using Temp.Models;
using Temp.Steps.Events;
using Temp.Steps.Functions;
using Temp.Steps.States;

namespace Temp.Steps;

public class RequestIntakeStep : KernelProcessStep
{
    [KernelFunction(RequestIntakeFunctions.CompleteRequestForm)]
    public virtual async ValueTask CompleteRequestFormAsync(KernelProcessStepContext context, RequestType requestType)
    {
        Console.Write("USER: " + requestType.Type.ToString());
        await Task.CompletedTask;
    }
}
