using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temp.Steps.Events
{
    public static class WelcomeEvents
    {
        public static readonly string StartProcess = nameof(StartProcess);
        public static readonly string WelcomeMessageDisplayComplete = nameof(WelcomeMessageDisplayComplete);
        public static readonly string RequestTypeSelectionComplete = nameof(RequestTypeSelectionComplete);
        public static readonly string RequestTypeCustomerInteractionTranscriptReady = nameof(RequestTypeCustomerInteractionTranscriptReady);
        public static readonly string RequestTypeIsNotValid = nameof(RequestTypeIsNotValid);
    }
}
