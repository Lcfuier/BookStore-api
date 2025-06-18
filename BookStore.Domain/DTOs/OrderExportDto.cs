using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Domain.DTOs
{
    public class OrderExportDto
    {
        public string OrderCode { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public DateTime? PaymentTime { get; set; }
        public string OrderStatus { get; set; }
        public string PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
        public decimal Total { get; set; }
    }
}
