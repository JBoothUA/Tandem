using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace TandemUserService
{
    public class TandemUserDto
    {
        public Guid UserId { get; set; }

        [Required]
        public string Name { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
