using IAM_Provisioning_Demo.Models;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAM_Provisioning_Demo.Plugins
{
    /// <summary>Request intake plugin: validates & normalizes incoming requests.</summary>
    public class RequestIntakePlugin
    {
        [KernelFunction("intake_request"), Description("Validate and normalize an IAM request.")]
        public IamRequest Intake(IamRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Id)) throw new ArgumentException("Request.Id required");
            if (req.Type == RequestType.GroupMembership && (string.IsNullOrWhiteSpace(req.TargetUser) || string.IsNullOrWhiteSpace(req.GroupName)))
                throw new ArgumentException("Membership needs TargetUser and GroupName");
            if (req.Type == RequestType.Group && string.IsNullOrWhiteSpace(req.GroupName))
                throw new ArgumentException("Group creation needs GroupName");

            // Example: infer OU defaults
            var attrs = req.Attributes ?? new Dictionary<string, string>();
            if (!attrs.ContainsKey("ou"))
            {
                attrs["ou"] = req.Type switch
                {
                    RequestType.UserAccount => Seed.DirectoryConfig.UsersOu,
                    RequestType.ServiceAccount => Seed.DirectoryConfig.ServicesOu,
                    RequestType.Group => Seed.DirectoryConfig.GroupsOu,
                    _ => Seed.DirectoryConfig.UsersOu
                };
            }

            return req with { Attributes = attrs };
        }
    }
}
