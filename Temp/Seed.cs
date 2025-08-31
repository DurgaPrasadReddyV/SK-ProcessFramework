using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temp
{
    public static class Seed // in-memory seed data (simulates HR/CMDB/Ownership)
    {
        // Users and their managers
        public static readonly Dictionary<string, string> ManagerOfUser = new()
        {
            ["alice"] = "carol",  // alice’s manager is carol
            ["svc-web"] = "opslead",
            ["bob"] = "carol",
            ["carol"] = "cto"
        };

        // Resource owners for account types or systems
        public static readonly Dictionary<string, string> ResourceOwnerOfApp = new()
        {
            ["AOS"] = "resowner-users",
            ["CXZ"] = "resowner-services",
        };

        // Platform owner (e.g., AD Platform Owner)
        public const string PlatformOwner = "ad-platform-owner";

        // Simple directory config (fill these for real env)
        public static class DirectoryConfig
        {
            public const string Domain = "contoso.local";
            public const string UsersOu = "OU=Users,DC=contoso,DC=local";
            public const string ServicesOu = "OU=Services,DC=contoso,DC=local";
            public const string GroupsOu = "OU=Groups,DC=contoso,DC=local";
            public const string AdminUser = "CONTOSO\\ad-provisioner";
            public const string AdminPassword = "P@ssw0rd!"; // store securely in practice
        }
    }
}
