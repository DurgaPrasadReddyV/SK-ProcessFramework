using Microsoft.SemanticKernel;

namespace Chapter_1.Steps;

public sealed class StartStep : KernelProcessStep
{
    [KernelFunction]
    public async ValueTask ExecuteAsync(KernelProcessStepContext context)
    {
        await Task.Delay(500); // Simulate some asynchronous work
        Console.WriteLine("Step 1 - Start\n");
    }
}
