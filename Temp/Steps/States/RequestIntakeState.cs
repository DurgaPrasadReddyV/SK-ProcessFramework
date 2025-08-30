using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temp.Models;

namespace Temp.Steps.States
{
    public class RequestIntakeState
    {
        public ERequestType? RequestType { get; set; }
        public ServiceAccountRequest? ServiceAccountRequest { get; set; }
        public UserAccountRequest? UserAccountRequest { get; set; }
        public DirectoryGroupRequest? DirectoryGroupRequest { get; set; }
        public DirectoryGroupMembershipRequest? DirectoryGroupMembershipRequest { get; set; }
        public List<ChatMessageContent> Conversation { get; set; } = [];
    }
}
