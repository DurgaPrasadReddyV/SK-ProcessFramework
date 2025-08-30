using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temp.Models;

namespace Temp.Steps.States
{
    public class WelcomeState
    {
        internal RequestType RequestType { get; set; } = new();
        internal List<ChatMessageContent> Conversation { get; set; } = [];
    }
}
