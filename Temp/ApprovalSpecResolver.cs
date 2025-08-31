using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temp.Models;

namespace Temp
{
    public static class ApprovalSpecResolver
    {
        public static ApprovalSpec ResolveServiceAccountRequest(ApprovalSpecDraft draft, ServiceAccountRequest req)
        {
            var required = new List<Approval>();

            foreach (var s in draft.DraftSteps)
            {
                string? approver = s.Selector switch
                {
                    "PlatformOwner()" => Seed.PlatformOwner,

                    var sel when sel.StartsWith("ResourceOwnerOf(", StringComparison.OrdinalIgnoreCase) =>
                        ResolveResourceOwnerOf(req.AppName),

                    _ => null
                };

                if (string.IsNullOrWhiteSpace(approver))
                    throw new InvalidOperationException($"Could not resolve selector '{s.Selector}' for role '{s.Role}'.");
                
                required.Add(new Approval() { Id = req.Id, Approver = approver, Role = s.Role});
            }

            return new ApprovalSpec(req.Id, required);
        }

        public static ApprovalSpec ResolveUserAccountRequest(ApprovalSpecDraft draft, UserAccountRequest req)
        {
            var required = new List<Approval>();

            foreach (var s in draft.DraftSteps)
            {
                string? approver = s.Selector switch
                {
                    "PlatformOwner()" => Seed.PlatformOwner,

                    var sel when sel.StartsWith("ManagerOf(", StringComparison.OrdinalIgnoreCase) =>
                        ResolveManagerOf(req.AccountName),

                    _ => null
                };

                if (string.IsNullOrWhiteSpace(approver))
                    throw new InvalidOperationException($"Could not resolve selector '{s.Selector}' for role '{s.Role}'.");

                required.Add(new Approval() { Id = req.Id, Approver = approver, Role = s.Role });
            }

            return new ApprovalSpec(req.Id, required);
        }

        private static string? ResolveManagerOf(string? user) =>
            string.IsNullOrWhiteSpace(user) ? null :
            Seed.ManagerOfUser.TryGetValue(user, out var m) ? m : null;

        private static string? ResolveResourceOwnerOf(string? appName) =>
            string.IsNullOrWhiteSpace(appName) ? null :
            Seed.ResourceOwnerOfApp.TryGetValue(appName, out var owner) ? owner : null;
    }

}
