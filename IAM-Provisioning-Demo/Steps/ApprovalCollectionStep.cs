using IAM_Provisioning_Demo.Models;
using IAM_Provisioning_Demo.Plugins;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAM_Provisioning_Demo.Steps
{
    // Step 3: Collect approvals
    public class ApprovalCollectionStep : KernelProcessStep
    {
        [KernelFunction]
        public List<Approval> Collect(IamRequest req, ApprovalSpec spec, [FromKernelServices] ApprovalPlugin approvals, [FromKernelServices] NotificationPlugin notify, [FromKernelServices] AuditPlugin audit)
        {
            foreach (var (person, role) in spec.RequiredApprovers)
                notify.Notify(person, $"Approval needed: {req.Type}/{req.Id}", $"Please review {req}");

            var results = approvals.CollectApprovals(req, spec);
            var allApproved = results.All(a => a.Approved);

            audit.Audit($"Approvals {req.Id} status={(allApproved ? "APPROVED" : "REJECTED")} details=[{string.Join(", ", results.Select(a => $"{a.Role}:{a.Approver}:{a.Approved}"))}]");

            if (!allApproved) throw new InvalidOperationException($"Request {req.Id} not approved");

            return results;
        }
    }
}
