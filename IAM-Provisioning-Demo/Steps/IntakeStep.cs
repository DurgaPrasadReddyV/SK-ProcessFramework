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
    // Step 1: Intake
    public class IntakeStep : KernelProcessStep
    {
        [KernelFunction]
        public IamRequest Intake(IamRequest req, [FromKernelServices] RequestIntakePlugin intake, [FromKernelServices] AuditPlugin audit)
        {
            var normalized = intake.Intake(req);
            audit.Audit($"Intake {normalized.Id} type={normalized.Type}");
            return normalized;
        }
    }
}
