using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter_3.Events;

public static class CommonEvents
{
    public static readonly string UserInputReceived = nameof(UserInputReceived);
    public static readonly string UserInputComplete = nameof(UserInputComplete);
    public static readonly string AssistantResponseGenerated = nameof(AssistantResponseGenerated);
    public static readonly string Exit = nameof(Exit);
}

