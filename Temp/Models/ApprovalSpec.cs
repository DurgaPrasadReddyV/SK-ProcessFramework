using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Temp.Models
{
    public class ApprovalSpec
    {
        public ApprovalSpec(Guid requestId, List<Approval> requiredApprovals)
        {
            RequestId = requestId;
            RequiredApprovals = requiredApprovals;
        }

        public Guid Id { get; set; } = Guid.NewGuid();
        
        [JsonPropertyName("requestId")]
        public Guid RequestId { get; set; }
        
        [JsonPropertyName("requiredApprovals")]
        public List<Approval> RequiredApprovals { get; set; } = new();
    }
}
