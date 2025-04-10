using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Domain.DTOs
{
    public class EnableTwoFaReq
    {
        [Required]
        [StringLength(6)]
        public string Passcode { get; set; }

    }
}
