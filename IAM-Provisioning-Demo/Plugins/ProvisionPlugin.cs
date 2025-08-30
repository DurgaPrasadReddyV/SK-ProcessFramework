using IAM_Provisioning_Demo.Models;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices.AccountManagement;
using System.Management.Automation;

namespace IAM_Provisioning_Demo.Plugins
{
    /// <summary>Provisioning plugin: OPTION A (C# DirectoryServices) & OPTION B (PowerShell).</summary>
    public class ProvisioningPlugin
    {
        // ====== OPTION A: pure C# AccountManagement ======
        [KernelFunction("provision_user_csharp")]
        public ProvisioningResult ProvisionUserCSharp(IamRequest req)
        {
            if (req.TargetUser is null) return new ProvisioningResult(req.Id, false, "TargetUser required");

            try
            {
                using var ctx = new PrincipalContext(ContextType.Domain, Seed.DirectoryConfig.Domain, Seed.DirectoryConfig.UsersOu,
                    ContextOptions.Negotiate, Seed.DirectoryConfig.AdminUser, Seed.DirectoryConfig.AdminPassword);

                using var user = new UserPrincipal(ctx)
                {
                    SamAccountName = req.TargetUser,
                    DisplayName = req.Attributes?.GetValueOrDefault("displayName") ?? req.TargetUser,
                    EmailAddress = req.Attributes?.GetValueOrDefault("mail")
                };

                string pwd = req.Attributes?.GetValueOrDefault("password") ?? "Temp#2025!";
                user.SetPassword(pwd);
                user.Enabled = true;
                user.Save();

                return new ProvisioningResult(req.Id, true, $"User {req.TargetUser} created (C#).");
            }
            catch (Exception ex)
            {
                return new ProvisioningResult(req.Id, false, ex.Message);
            }
        }

        [KernelFunction("provision_group_csharp")]
        public ProvisioningResult ProvisionGroupCSharp(IamRequest req)
        {
            if (req.GroupName is null) return new ProvisioningResult(req.Id, false, "GroupName required");

            try
            {
                using var ctx = new PrincipalContext(ContextType.Domain, Seed.DirectoryConfig.Domain, Seed.DirectoryConfig.GroupsOu,
                    ContextOptions.Negotiate, Seed.DirectoryConfig.AdminUser, Seed.DirectoryConfig.AdminPassword);

                using var grp = new GroupPrincipal(ctx, req.GroupName);
                grp.GroupScope = GroupScope.Global;
                grp.IsSecurityGroup = true;
                grp.Save();

                return new ProvisioningResult(req.Id, true, $"Group {req.GroupName} created (C#).");
            }
            catch (Exception ex)
            {
                return new ProvisioningResult(req.Id, false, ex.Message);
            }
        }

        [KernelFunction("add_membership_csharp")]
        public ProvisioningResult AddMembershipCSharp(IamRequest req)
        {
            if (req.TargetUser is null || req.GroupName is null)
                return new ProvisioningResult(req.Id, false, "Membership needs TargetUser & GroupName");

            try
            {
                using var ctx = new PrincipalContext(ContextType.Domain, Seed.DirectoryConfig.Domain, null,
                    ContextOptions.Negotiate, Seed.DirectoryConfig.AdminUser, Seed.DirectoryConfig.AdminPassword);

                var user = UserPrincipal.FindByIdentity(ctx, IdentityType.SamAccountName, req.TargetUser);
                var group = GroupPrincipal.FindByIdentity(ctx, IdentityType.SamAccountName, req.GroupName);
                if (user == null || group == null) return new ProvisioningResult(req.Id, false, "User or group not found");

                group.Members.Add(user);
                group.Save();

                return new ProvisioningResult(req.Id, true, $"Added {req.TargetUser} to {req.GroupName} (C#).");
            }
            catch (Exception ex)
            {
                return new ProvisioningResult(req.Id, false, ex.Message);
            }
        }

        // ====== OPTION B: PowerShell ActiveDirectory module ======
        [KernelFunction("provision_user_ps")]
        public ProvisioningResult ProvisionUserPs(IamRequest req)
        {
            if (req.TargetUser is null) return new ProvisioningResult(req.Id, false, "TargetUser required");
            var ou = req.Attributes?.GetValueOrDefault("ou") ?? Seed.DirectoryConfig.UsersOu;
            var pwd = req.Attributes?.GetValueOrDefault("password") ?? "Temp#2025!";

            // New-ADUser docs. :contentReference[oaicite:4]{index=4}
            var script = $@"
Import-Module ActiveDirectory
New-ADUser -Name '{req.TargetUser}' -SamAccountName '{req.TargetUser}' -Path '{ou}' -Enabled $true -AccountPassword (ConvertTo-SecureString '{pwd}' -AsPlainText -Force)
";

            return InvokePs(script, req.Id, $"User {req.TargetUser} created (PS).");
        }

        [KernelFunction("provision_group_ps")]
        public ProvisioningResult ProvisionGroupPs(IamRequest req)
        {
            if (req.GroupName is null) return new ProvisioningResult(req.Id, false, "GroupName required");
            var ou = req.Attributes?.GetValueOrDefault("ou") ?? Seed.DirectoryConfig.GroupsOu;

            // New-ADGroup docs. :contentReference[oaicite:5]{index=5}
            var script = $@"
Import-Module ActiveDirectory
New-ADGroup -Name '{req.GroupName}' -GroupScope Global -Path '{ou}' -SamAccountName '{req.GroupName}' -GroupCategory Security
";

            return InvokePs(script, req.Id, $"Group {req.GroupName} created (PS).");
        }

        [KernelFunction("add_membership_ps")]
        public ProvisioningResult AddMembershipPs(IamRequest req)
        {
            if (req.TargetUser is null || req.GroupName is null)
                return new ProvisioningResult(req.Id, false, "Membership needs TargetUser & GroupName");

            // Add-ADGroupMember docs. :contentReference[oaicite:6]{index=6}
            var script = $@"
Import-Module ActiveDirectory
Add-ADGroupMember -Identity '{req.GroupName}' -Members '{req.TargetUser}'
";

            return InvokePs(script, req.Id, $"Added {req.TargetUser} to {req.GroupName} (PS).");
        }

        private static ProvisioningResult InvokePs(string psScript, string requestId, string okMessage)
        {
            try
            {
                using var ps = PowerShell.Create();
                ps.AddScript(psScript);
                var results = ps.Invoke();
                if (ps.Streams.Error.Count > 0)
                {
                    var err = string.Join(" | ", ps.Streams.Error.Select(e => e.ToString()));
                    return new ProvisioningResult(requestId, false, err);
                }
                return new ProvisioningResult(requestId, true, okMessage);
            }
            catch (Exception ex)
            {
                return new ProvisioningResult(requestId, false, ex.Message);
            }
        }
    }
}
