using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temp.Steps.Events
{
    public class RequestIntakeEvents
    {
        public static readonly string RequestTypeCalloutComplete = nameof(RequestTypeCalloutComplete);
        public static readonly string ServiceAccountRequestFormComplete = nameof(ServiceAccountRequestFormComplete);
        public static readonly string ServiceAccountRequestFormNeedsMoreDetails = nameof(ServiceAccountRequestFormNeedsMoreDetails);
        public static readonly string ServiceAccountRequestCustomerInteractionTranscriptReady = nameof(ServiceAccountRequestCustomerInteractionTranscriptReady);
    }
}
