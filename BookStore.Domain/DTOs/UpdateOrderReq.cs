using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Domain.DTOs
{
    public class UpdateOrderReq
    {
        public string? OrderId {  get; set; }
        public string OrderStatus { get; set; }
        public string? Carrier {  get; set; }
        public string? TrackingNumber { get; set; }
    }
}
