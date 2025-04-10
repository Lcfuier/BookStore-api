using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Domain.Constants.VnPay
{
    public class VnPayRequestModel
    {
        public string Name { get; set; }
        public string OrderId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public bool StoreCard = false;
    }
}
