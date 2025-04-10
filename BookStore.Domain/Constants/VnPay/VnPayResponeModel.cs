using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Domain.Constants.VnPay
{
    public class VnPayResponeModel
    {
        public bool IsSuccess { get; set; }
        public string PaymentMethod { get; set; }
        public string BookingDescription { get; set; }
        public string BookingId { get; set; }
        public string PaymentId { get; set; }
        public string TransactionId { get; set; }
        public string Token { get; set; }
        public string TxnRef { get; set; }
        public string VnPayResponeCode { get; set; }
        public string userName { get; set; }
        public string paymentStatus { get; set; }

    }
}
