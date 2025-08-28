using Chapter_2.Steps;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter_2.States;

/// <summary>
/// The state object for the <see cref="ChatBotResponseStep"/>.
/// </summary>
public class ChatBotState
{
    internal ChatHistory ChatMessages { get; } = new();
}
