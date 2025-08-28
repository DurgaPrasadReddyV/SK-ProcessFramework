using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter_2.Events;

/// <summary>
/// A class that defines the events that can be emitted by the chat bot process. This is
/// not required but used to ensure that the event names are consistent.
/// </summary>
public static class ChatBotEvents
{
    public const string StartProcess = "startProcess";
    public const string IntroComplete = "introComplete";
    public const string AssistantResponseGenerated = "assistantResponseGenerated";
    public const string Exit = "exit";
}

