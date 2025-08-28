using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter_2.Steps;

/// <summary>
/// The simplest implementation of a process step. IntroStep
/// </summary>
public class IntroStep : KernelProcessStep
{
    /// <summary>
    /// Prints an introduction message to the console.
    /// </summary>
    [KernelFunction]
    public void PrintIntroMessage()
    {
        System.Console.WriteLine("Welcome to Processes in Semantic Kernel.\n");
    }
}
