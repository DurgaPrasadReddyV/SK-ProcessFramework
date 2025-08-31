using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temp.Models
{
    public class PolicyApproverStep
    {
        public PolicyApproverStep(string role, string selector)
        {
            Role = role;
            Selector = selector;
        }

        public string Role { get; set; } = string.Empty; // canonical: Manager, GroupOwner, ResourceOwner, PlatformOwner, SecurityOwner, etc.
        public string Selector { get; set; } = string.Empty; // abstract selector, ManagerOf(TargetUser), GroupOwnerOf(GroupName), ResourceOwnerOf(TargetService|RequestType), PlatformOwner()
    }
}
