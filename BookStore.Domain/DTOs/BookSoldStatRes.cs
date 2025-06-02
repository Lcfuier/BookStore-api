using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Domain.DTOs
{
    public class BookSoldStatRes
    {
        public DateTime Date { get; set; }
        public int TotalBooksSold { get; set; }
    }
}
