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
    // Step 4: Provision (choose OPTION A C# or OPTION B PS per request type)
    public class ProvisioningStep : KernelProcessStep
    {
        [KernelFunction]
        public ProvisioningResult Provision(IamRequest req, [FromKernelServices] ProvisioningPlugin prov, [FromKernelServices] AuditPlugin audit)
        {
            ProvisioningResult result = req.Type switch
            {
                RequestType.UserAccount => prov.ProvisionUserCSharp(req),           // or prov.ProvisionUserPs(req)
                RequestType.ServiceAccount => prov.ProvisionUserCSharp(req),           // treat as user in Services OU
                RequestType.Group => prov.ProvisionGroupCSharp(req),          // or prov.ProvisionGroupPs(req)
                RequestType.GroupMembership => prov.AddMembershipCSharp(req),           // or prov.AddMembershipPs(req)
                _ => new ProvisioningResult(req.Id, false, "Unknown type")
            };

            audit.Audit($"Provision {req.Id} success={result.Success} msg={result.Message}");
            return result;
        }
    }
}
