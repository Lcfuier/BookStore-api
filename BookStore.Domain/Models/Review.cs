using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Domain.Models
{
    public class Review
    {
        [Key]
        public Guid Id { get; set; }
        public string UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        [ValidateNever]
        public ApplicationUser? User { get; set; }
        public Guid BookId { get; set; }
        public Book? Book { get; set; }
        [Column(TypeName = "decimal(2,1)")]
        [Range(1.0, 5.0, ErrorMessage = "Rating must be between 1.0 and 5.0")]
        public decimal Rating { get; set; }
        public string Comment { get; set; }
        [NotMapped]
        public string userfullName => this.User.FullName;
        //audit
        public DateTime? CreatedTime { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedTime { get; set; }
        public string? ModifiedBy { get; set; }
        //

    }
}
