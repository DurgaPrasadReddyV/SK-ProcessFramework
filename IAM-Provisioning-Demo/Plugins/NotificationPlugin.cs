using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAM_Provisioning_Demo.Plugins
{
    /// <summary>Notification plugin (stub): send emails/Slack, here we just Console.WriteLine.</summary>
    public class NotificationPlugin
    {
        [KernelFunction("notify"), Description("Notify a principal about approval or provisioning status.")]
        public void Notify(string to, string subject, string body)
        {
            Console.WriteLine($"[NOTIFY] To={to} | {subject}\n{body}\n");
        }
    }
}
