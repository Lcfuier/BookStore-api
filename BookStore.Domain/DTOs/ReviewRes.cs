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
    public class ReviewRes
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public Guid BookId { get; set; }
        public decimal Rating { get; set; }
        public string Comment { get; set; }
        [NotMapped]
        public string userfullName { get; set; }
        //audit
        public DateTime? CreatedTime { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedTime { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
