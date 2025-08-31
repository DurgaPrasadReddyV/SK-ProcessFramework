using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temp.Models
{
    public class Approval
    {
        Guid Id { get; set; } = Guid.NewGuid();
        public string Approver { get; set; } = string.Empty; // User's email
        public string Role { get; set; } = string.Empty;      // Manager, ResourceOwner, PlatformOwner, GroupOwner
        public bool Approved { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}
