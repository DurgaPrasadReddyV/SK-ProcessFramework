using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temp.Models
{
    public class RequestType
    {
        public ERequestType Type { get; set; }

        public bool IsValid()
        {
            return Type != ERequestType.Unknown;
        }
    }

    public enum ERequestType
    {
        Unknown = 0,
        CreateServiceAccount = 1,
        CreateUserAccount = 2,
        ManageGroupMembership = 3,
        CreateGroup = 4
    }
}
