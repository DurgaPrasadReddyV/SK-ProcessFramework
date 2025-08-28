using Chapter_1.Steps;
using Microsoft.SemanticKernel;

namespace Chapter_1
{
    internal class Program
    {
        public static class ProcessEvents
        {
            public const string StartProcess = nameof(StartProcess);
        }

        static async Task Main(string[] args)
        {
            // Create a simple kernel 
            Kernel kernel = Kernel.CreateBuilder()
                .Build();

            // Create a process that will interact with the chat completion service
            ProcessBuilder process = new("ChatBot");
            var startStep = process.AddStepFromType<StartStep>();
            var doSomeWorkStep = process.AddStepFromType<DoSomeWorkStep>();
            var doMoreWorkStep = process.AddStepFromType<DoMoreWorkStep>();
            var lastStep = process.AddStepFromType<LastStep>();

            // Define the process flow
            process
                .OnInputEvent(ProcessEvents.StartProcess)
                .SendEventTo(new ProcessFunctionTargetBuilder(startStep));

            startStep
                .OnFunctionResult()
                .SendEventTo(new ProcessFunctionTargetBuilder(doSomeWorkStep));

            doSomeWorkStep
                .OnFunctionResult()
                .SendEventTo(new ProcessFunctionTargetBuilder(doMoreWorkStep));

            doMoreWorkStep
                .OnFunctionResult()
                .SendEventTo(new ProcessFunctionTargetBuilder(lastStep));

            lastStep
                .OnFunctionResult()
                .StopProcess();

            // Build the process to get a handle that can be started
            KernelProcess kernelProcess = process.Build();

            // Start the process with an initial external event
            await using var runningProcess = await kernelProcess.StartAsync(
                kernel,
                    new KernelProcessEvent()
                    {
                        Id = ProcessEvents.StartProcess,
                        Data = null
                    });
        }
    }
}
