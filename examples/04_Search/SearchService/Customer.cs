using LiteDB;
using Newtonsoft.Json;
using System;

namespace SearchService
{
    public class Customer
    {
        [BsonId]
        [JsonIgnore]
        public ObjectId CustomerId { get; set; }

        [BsonIgnore]
        [JsonProperty(PropertyName = "id")]
        public string Id => CustomerId.ToString();

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        public void TrimAll()
        {
            Name = Name?.Trim();
            Email = Email?.Trim();
            Country = Country?.Trim();
        }

        public bool HasError(out string errorMsg)
        {
            // Validate Name
            if (Name != null && Name == "")
            {
                errorMsg = "Name must not be empty.";
                return true;
            }

            // Validate Email (allow empty)
            if (!String.IsNullOrEmpty(Email) && !HasValidEmail())
            {
                errorMsg = "Invalid email address.";
                return true;
            }

            // Validate Country (allow empty)
            if (!String.IsNullOrEmpty(Country) && !Countries.List.Contains(Country))
            {
                errorMsg = "Country must be empty or one of the following: " + string.Join(", ", Countries.List) + ".";
                return true;
            }

            errorMsg = null;
            return false;
        }

        public bool HasValidEmail()
        {
            if (string.IsNullOrEmpty(Email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(Email);
                return addr.Address == Email;
            }
            catch
            {
                return false;
            }
        }
    }
}
