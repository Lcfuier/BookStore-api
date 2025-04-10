using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Domain.Models
{
    public class OrderItem
    {
        [Key]
        public Guid OrderDetailId { get; set; }
        public int Quantity { get; set; }
        [Column(TypeName = "Money")]
        public Decimal Price { get; set; }
        public Guid BookId { get; set; }
        public Book? Book { get; set; }

        public Guid OrderId { get; set; }
        public Order? Order { get; set; }
    }
}
