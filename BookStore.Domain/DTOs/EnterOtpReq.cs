using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Domain.DTOs
{
    public class EnterOtpReq
    {
        [Required]
        public string Otp { get; set; }
        [Required]
        public string UserName { get; set; }
    }
}
