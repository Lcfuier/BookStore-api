using BookStore.Domain.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Domain.DTOs
{
    public class AddReviewReq
    {
        public Guid BookId { get; set; }
        [Column(TypeName = "decimal(2,1)")]
        [Range(1.0, 5.0, ErrorMessage = "Rating must be between 1.0 and 5.0")]
        public decimal Rating { get; set; }
        public string Comment { get; set; }
    }
}
