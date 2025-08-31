using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Temp.Models
{
    public class UserAccountRequest
    {
        [JsonPropertyName("accountName")]
        public string AccountName { get; set; } = string.Empty;

        public UserAccountRequest CopyWithDefaultValues(string defaultStringValue = "Unanswered")
        {
            UserAccountRequest copy = new();
            PropertyInfo[] properties = typeof(UserAccountRequest).GetProperties();

            foreach (PropertyInfo property in properties)
            {
                // Get the value of the property  
                string? value = property.GetValue(this) as string;

                // Check if the value is an empty string  
                if (string.IsNullOrEmpty(value))
                {
                    property.SetValue(copy, defaultStringValue);
                }
                else
                {
                    property.SetValue(copy, value);
                }
            }

            return copy;
        }

        public bool IsFormCompleted()
        {
            return !string.IsNullOrEmpty(AccountName);
        }
    }
}
