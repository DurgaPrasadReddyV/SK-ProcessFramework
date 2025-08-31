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
        public Guid Id { get; set; } = Guid.NewGuid();

        [JsonPropertyName("accountName")]
        public string AccountName { get; set; } = string.Empty;

        [JsonPropertyName("appName")]
        public string AppName { get; set; } = string.Empty;

        [JsonPropertyName("domainName")]
        public string DomainName { get; set; } = string.Empty;

        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("passwordNeverExpires")]
        public string PasswordNeverExpires { get; set; } = "false";

        [JsonPropertyName("passwordToBeVaulted")]
        public string PasswordToBeVaulted { get; set; } = "false";

        public UserAccountRequest CopyWithDefaultValues(string defaultStringValue = "Unanswered")
        {
            UserAccountRequest copy = new();
            PropertyInfo[] properties = typeof(UserAccountRequest).GetProperties();

            foreach (PropertyInfo property in properties)
            {
                // Exclude Id from being copied
                if (property.Name == nameof(Id))
                    continue;

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
            return !string.IsNullOrEmpty(AccountName) && !string.IsNullOrEmpty(AppName) && !string.IsNullOrEmpty(DomainName) && !string.IsNullOrEmpty(UserId);
        }
    }
}
