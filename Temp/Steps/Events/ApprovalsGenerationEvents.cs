using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temp.Steps.Events
{
    public class ApprovalsGenerationEvents
    {
        public static readonly string ServiceAccountApprovalsGenerated = nameof(ServiceAccountApprovalsGenerated);
        public static readonly string UserAccountApprovalsGenerated = nameof(UserAccountApprovalsGenerated);
    }
}
