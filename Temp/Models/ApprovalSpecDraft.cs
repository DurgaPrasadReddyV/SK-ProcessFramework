using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temp.Models
{
    public class ApprovalSpecDraft
    {
        public Guid RequestId { get; set; }
        public List<ApprovalSpecDraftStep> DraftSteps { get; set; } = new();
    }
}
