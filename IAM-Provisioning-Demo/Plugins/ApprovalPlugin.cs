using IAM_Provisioning_Demo.Models;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAM_Provisioning_Demo.Plugins
{
    /// <summary>Approval routing plugin: decides required approvers based on request type and seed data.</summary>
    public class ApprovalPlugin
    {
        [KernelFunction("build_approval_spec"), Description("Build approver list and auto-approval for a request.")]
        public ApprovalSpec BuildApprovalSpec(IamRequest req)
        {
            var required = new List<(string, string)>();
            bool autoApprove = false;

            switch (req.Type)
            {
                case RequestType.UserAccount:
                case RequestType.ServiceAccount:
                    // Resource Owner + Platform Owner
                    var ro = Seed.ResourceOwnerBySystem[(req.Type == RequestType.UserAccount) ? "UserAccount" : "ServiceAccount"];
                    required.Add((ro, "ResourceOwner"));
                    required.Add((Seed.PlatformOwner, "PlatformOwner"));
                    break;

                case RequestType.Group:
                    if (req.GroupName is null) throw new ArgumentException("Group name required");
                    var go = Seed.GroupOwner[req.GroupName];
                    required.Add((go, "GroupOwner"));
                    required.Add((Seed.PlatformOwner, "PlatformOwner"));
                    break;

                case RequestType.GroupMembership:
                    if (req.TargetUser is null || req.GroupName is null)
                        throw new ArgumentException("Membership needs TargetUser & GroupName");

                    // User’s manager + Group Owner
                    var mgr = Seed.ManagerOfUser.ContainsKey(req.TargetUser) ? Seed.ManagerOfUser[req.TargetUser] : "manager-not-found";
                    var gOwner = Seed.GroupOwner[req.GroupName];
                    required.Add((mgr, "Manager"));
                    required.Add((gOwner, "GroupOwner"));

                    // Demo auto-approval rule
                    if (Seed.AutoJoinGroups.Contains(req.GroupName)) autoApprove = true;
                    break;
            }

            return new ApprovalSpec(req.Id, required, autoApprove);
        }

        [KernelFunction("collect_approvals"), Description("Simulate approval collection; auto-approve if allowed.")]
        public List<Approval> CollectApprovals(IamRequest req, ApprovalSpec spec)
        {
            var now = DateTimeOffset.UtcNow;
            var approvals = new List<Approval>();

            if (spec.AutoApproved)
            {
                approvals = spec.RequiredApprovers.Select(a => new Approval(a.Approver, a.Role, true, now)).ToList();
                return approvals;
            }

            // Demo: “manager” always approves, “owners” approve unless requestor equals approver (prevent self-approval)
            foreach (var (approver, role) in spec.RequiredApprovers)
            {
                bool ok = !string.Equals(req.RequestedBy, approver, StringComparison.OrdinalIgnoreCase);
                approvals.Add(new Approval(approver, role, ok, now));
            }

            return approvals;
        }
    }
}
