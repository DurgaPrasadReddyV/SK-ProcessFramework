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
        public RequestType RequestType { get; set; } = new();
        public List<ChatMessageContent> Conversation { get; set; } = [];
    }
}
