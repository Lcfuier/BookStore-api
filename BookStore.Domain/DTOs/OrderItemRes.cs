using BookStore.Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Domain.DTOs
{
    public class OrderItemRes
    {
        public Guid OrderDetailId { get; set; }
        public int Quantity { get; set; }
        [Column(TypeName = "Money")]
        public Decimal Price { get; set; }
        public Guid BookId { get; set; }
        public Book? Book { get; set; }
    }
}
