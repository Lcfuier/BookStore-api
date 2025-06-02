using BookStore.Domain.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Domain.DTOs
{
    public class GetPaymentProfileRes
    {
        public Guid PaymentProfileId { get; set; }
        public string UserId { get; set; }
        public string? Token { get; set; }
        public string? Nickname { get; set; }
        //audit
        public DateTime? CreatedTime { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedTime { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
