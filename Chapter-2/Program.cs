using Chapter_2.Events;
using Chapter_2.Steps;
using Chapter_2.Utilities;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Process.Tools;

namespace Chapter_2
{
    public class Program
    {
        public static class ProcessEvents
        {
            public const string StartProcess = nameof(StartProcess);
        }

        static async Task Main(string[] args)
        {
            // Create a google kernel 
            var kernelBuilder = Kernel.CreateBuilder()
            .AddGoogleAIGeminiChatCompletion(modelId: "gemini-2.5-flash", apiKey: "");

            Kernel kernel = kernelBuilder.Build();

            // Create a process that will interact with the chat completion service
            ProcessBuilder process = new("ChatBot");
            var introStep = process.AddStepFromType<IntroStep>();
            var userInputStep = process.AddStepFromType<ChatUserInputStep>();
            var responseStep = process.AddStepFromType<ChatBotResponseStep>();

            // Define the behavior when the process receives an external event
            process
                .OnInputEvent(ChatBotEvents.StartProcess)
                .SendEventTo(new ProcessFunctionTargetBuilder(introStep));

            // When the intro is complete, notify the userInput step
            introStep
                .OnFunctionResult()
                .SendEventTo(new ProcessFunctionTargetBuilder(userInputStep));

            // When the userInput step emits an exit event, send it to the end step
            userInputStep
                .OnEvent(ChatBotEvents.Exit)
                .StopProcess();

            // When the userInput step emits a user input event, send it to the assistantResponse step
            userInputStep
                .OnEvent(CommonEvents.UserInputReceived)
                .SendEventTo(new ProcessFunctionTargetBuilder(responseStep, parameterName: "userMessage"));

            // When the assistantResponse step emits a response, send it to the userInput step
            responseStep
                .OnEvent(ChatBotEvents.AssistantResponseGenerated)
                .SendEventTo(new ProcessFunctionTargetBuilder(userInputStep));

            // Build the process to get a handle that can be started
            KernelProcess kernelProcess = process.Build();

            // Generate a Mermaid diagram for the process and print it to the console
            string mermaidGraph = kernelProcess.ToMermaid();
            Console.WriteLine($"=== Start - Mermaid Diagram for '{process.Name}' ===");
            Console.WriteLine(mermaidGraph);
            Console.WriteLine($"=== End - Mermaid Diagram for '{process.Name}' ===");

            // Generate an image from the Mermaid diagram
            string generatedImagePath = await MermaidRenderer.GenerateMermaidImageAsync(mermaidGraph, "ChatBotProcess.png");
            Console.WriteLine($"Diagram generated at: {generatedImagePath}");

            // Start the process with an initial external event
            await using var runningProcess = await kernelProcess.StartAsync(kernel, new KernelProcessEvent() { Id = ChatBotEvents.StartProcess, Data = null });

        }
    }
}
