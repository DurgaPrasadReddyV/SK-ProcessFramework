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
    // Step 5: Done/Notify requester
    public class CompletionStep : KernelProcessStep
    {
        [KernelFunction]
        public void Complete(IamRequest req, ProvisioningResult result, [FromKernelServices] NotificationPlugin notify)
        {
            var status = result.Success ? "Provisioned" : "Failed";
            notify.Notify(req.RequestedBy, $"Request {req.Id} {status}", result.Message);
        }
    }

}
