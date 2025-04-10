using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Domain.DTOs
{
    public class OrderInfoVnpay
    {
        public string OrderId { get; set; }
        public string UserName { get; set; }
        public string NickName { get; set; }
    }
}
