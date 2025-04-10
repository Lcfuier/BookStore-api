using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Domain.DTOs
{
    public class UpdateBookReq
    {
        [MaxLength(80)]
        public string Title { get; set; }
        public string Description { get; set; }
        [Column(TypeName = "varchar(13)")]
        public string Isbn13 { get; set; }
        public int Inventory { get; set; }
        [Column(TypeName = "Money")]
        public decimal Price { get; set; }
        [Required]
        public decimal DiscountPercent { get; set; }

        public int NumberOfPage { get; set; }
        public DateTime PublicationDate { get; set; }

        public Guid authorId { get; set; }

        public Guid publisherID { get; set; }

        //public ICollection<OrderDetail>? OrderDetails { get; set; }
        [ForeignKey("BookId")]
        [InverseProperty("Books")]
        public Guid[] CategoryIds { get; set; } = null!;
    }
}
