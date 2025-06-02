using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Domain.DTOs
{
    public class GetInformationRes
    {
        public string? PhoneNumber { get; set; }
        public string Email { get; set; }
        public bool? TwoFactorGoogleEnabled { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string userName { get; set; }
    }
}
