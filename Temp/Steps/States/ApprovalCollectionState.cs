using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temp.Models;

namespace Temp.Steps.States
{
    public class ApprovalCollectionState
    {
        public Guid RequestId { get; set; }
        public HashSet<Guid> Pending { get; set; } = new();
        public Dictionary<Guid, Approval> Received { get; set; } = new();
        public bool AllRequiredReceived => Pending.Count == 0;
        public bool AnyRejected => Received.Values.Any(r => r.Approved == false);
    }
}
