using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Domain.DTOs
{
    public class ShippingFeeRequest
    {
        public int ToDistrictId { get; set; }
        public string ToWardCode { get; set; }

        public int Weight { get; set; } = 500; // gram
        public int Length { get; set; } = 20;
        public int Width { get; set; } = 20;
        public int Height { get; set; } = 10;
    }
}
