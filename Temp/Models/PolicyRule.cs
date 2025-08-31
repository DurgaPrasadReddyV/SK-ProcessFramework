using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temp.Models
{
    public class PolicyRule
    {
        public PolicyRule(string requestType, List<PolicyApproverStep> steps)
        {
            RequestType = requestType;
            Steps = steps;
        }

        public string RequestType { get; set; } = string.Empty; // e.g., "UserAccount", "ServiceAccount", "Group", "GroupMembership", supports wildcard like "Membership:*"
        public List<PolicyApproverStep> Steps { get; set; } = new();
    }
}
