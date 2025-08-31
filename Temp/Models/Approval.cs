using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Temp.Models
{
    public class Approval
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [JsonPropertyName("requestId")]
        public Guid RequestId { get; set; }

        [JsonPropertyName("approver")]
        public string Approver { get; set; } = string.Empty; // User's email
        
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty; // Manager, ResourceOwner, PlatformOwner, GroupOwner
        
        [JsonPropertyName("approved")]
        public bool? Approved { get; set; } = null; // null = pending, true = approved, false = rejected

        [JsonPropertyName("timestamp")]
        public DateTimeOffset Timestamp { get; set; }
    }
}
