using BookStore.Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Domain.DTOs
{
    public class GetBookRes
    {
        [Key]
        public Guid BookId { get; set; }
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
        [NotMapped]
        public decimal PriceDiscount => Price - Price * DiscountPercent;

        public int NumberOfPage { get; set; }
        public DateTime PublicationDate { get; set; }
        public string ImageURL { get; set; }

        public Guid authorId { get; set; }
        public string? AuthorName { get; set; }

        public Guid publisherID { get; set; }
        public string? PublisherName { get; set; }

        //audit
        public DateTime? CreatedTime { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedTime { get; set; }
        public string? ModifiedBy { get; set; }
        //

        public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}
