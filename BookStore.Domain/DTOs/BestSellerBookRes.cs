using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Domain.DTOs
{
    public class BestSellerBookRes
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
     
        public decimal Price { get; set; }
        public decimal DiscountPercent { get; set; }
        public string ImageURL { get; set; }
        public string AuthorName { get; set; }
        public int TotalSold { get; set; }
    }
}
