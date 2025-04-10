using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Domain.DTOs
{
    public class TwoFactorAuthenticationRes
    {
        public string? QrCodeImageBase64 { get; set; }
        public string? ManualKey { get; set; }
    }
}
