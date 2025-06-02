using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Domain.DTOs
{
    public class RevenuePointRes
    {
        public DateTime Date { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
