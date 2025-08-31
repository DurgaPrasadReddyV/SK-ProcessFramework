using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temp.Steps.Events
{
    internal class ProvisioningEvents
    {
        public static readonly string ProvisionEngineerReviewReceived = nameof(ProvisionEngineerReviewReceived);
        public static readonly string ProvisionEngineerReviewApproved = nameof(ProvisionEngineerReviewApproved);
        public static readonly string ProvisionEngineerReviewRejected = nameof(ProvisionEngineerReviewRejected);
        public static readonly string ProcessCompleted = nameof(ProcessCompleted);
    }
}
