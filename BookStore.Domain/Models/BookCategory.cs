using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Domain.Models
{
    public class BookCategory
    {
        [Key]
        [Required]
        public Guid BookId { get; set; }
        [Key]
        [Required]
        public Guid CategoryId { get; set; }

        //audit
        public DateTime? CreatedTime { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedTime { get; set; }
        public string? ModifiedBy { get; set; }
        //

        public Book? Book { get; set; }
        public Category? Category { get; set; }
    }
}
