using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temp.Models
{
    public class ApprovalSpec
    {
        Guid Id { get; set; } = Guid.NewGuid();
        public Guid RequestId { get; set; }
        public List<Approval> RequiredApprovals { get; set; } = new();
    }
}
