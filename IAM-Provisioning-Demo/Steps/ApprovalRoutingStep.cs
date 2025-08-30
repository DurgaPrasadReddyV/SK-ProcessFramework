using IAM_Provisioning_Demo.Models;
using IAM_Provisioning_Demo.Plugins;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IAM_Provisioning_Demo.Steps.ApprovalRoutingStep;

namespace IAM_Provisioning_Demo.Steps
{
    // Step 2: Build approval spec
    public class ApprovalRoutingStep : KernelProcessStep<ApprovalRoutingState>
    {
        public class ApprovalRoutingState { public ApprovalSpec? Spec { get; set; } }

        private ApprovalRoutingState _state = new();

        public override ValueTask ActivateAsync(KernelProcessStepState<ApprovalRoutingState> state)
        {
            _state = state.State!;
            return base.ActivateAsync(state);
        }

        [KernelFunction]
        public ApprovalSpec Route(IamRequest req, [FromKernelServices] ApprovalPlugin approvals, [FromKernelServices] AuditPlugin audit)
        {
            _state.Spec = approvals.BuildApprovalSpec(req);
            audit.Audit($"Route {req.Id} approvers=[{string.Join(", ", _state.Spec.RequiredApprovers.Select(a => $"{a.Role}:{a.Approver}"))}] auto={_state.Spec.AutoApproved}");
            return _state.Spec;
        }
    }
}
