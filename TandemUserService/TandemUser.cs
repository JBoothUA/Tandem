using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace TandemUserService
{
    public class TandemUser
    {
        [JsonProperty("id")]
        public Guid Id { get; private set; }

        [Required]
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("middleName")]
        public string MiddleName { get; set; }

        [Required]
        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        [JsonProperty("emailAddress")]
        public string EmailAddress { get; set; }

        public void EnsureId()
        {
            if (Id == Guid.Empty)
            {
                Id = Guid.NewGuid();
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
