using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAM_Provisioning_Demo.Models
{
    public record ProvisioningResult(
        string RequestId,
        bool Success,
        string Message);
}
