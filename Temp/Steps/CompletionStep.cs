using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temp.Steps.Functions;

namespace Temp.Steps
{
    public class CompletionStep : KernelProcessStep
    {
        [KernelFunction(CompletionFunctions.Complete)]
        public void Complete(KernelProcessStepContext context, bool success)
        {
            var status = success ? "Provisioned" : "Failed";
            Console.WriteLine($"[COMPLETION]: {status}");
        }
    }
}
