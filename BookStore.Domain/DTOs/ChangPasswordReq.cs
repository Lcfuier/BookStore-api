using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Domain.DTOs
{
    public class ChangPasswordReq
    {
        [Required]
        [PasswordPropertyText]
        public string CurrentPassword { get; set; }
        [Required]
        [PasswordPropertyText]
        public string NewPassword { get; set; }
    }
}
