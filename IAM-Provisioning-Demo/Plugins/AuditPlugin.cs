using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAM_Provisioning_Demo.Plugins
{
    /// <summary>Audit plugin (stub): central audit trail.</summary>
    public class AuditPlugin
    {
        private static readonly List<string> _events = new();

        [KernelFunction("audit_event")]
        public void Audit(string evt)
        {
            _events.Add($"{DateTimeOffset.UtcNow:o} {evt}");
            Console.WriteLine($"[AUDIT] {evt}");
        }

        [KernelFunction("dump_audit")]
        public IEnumerable<string> Dump() => _events.ToList();
    }
}
