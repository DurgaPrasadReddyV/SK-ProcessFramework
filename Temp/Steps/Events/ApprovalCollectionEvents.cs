using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temp.Steps.Events
{
    internal class ApprovalCollectionEvents
    {
        public static readonly string ApprovalReceived = nameof(ApprovalReceived);
        public static readonly string ApprovalRecorded = nameof(ApprovalRecorded);
        public static readonly string AllApprovalsReceived = nameof(AllApprovalsReceived);
        public static readonly string AllApprovalsApproved = nameof(AllApprovalsApproved);
    }
}
