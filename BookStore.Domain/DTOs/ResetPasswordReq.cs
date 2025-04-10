using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Domain.DTOs
{
    public class ResetPasswordReq
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Otp { get; set; }
        [Required]
        [PasswordPropertyText]
        public string Password { get; set; }
    }
}
