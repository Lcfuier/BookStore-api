using BookStore.Domain.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Domain.DTOs
{
    public class OrderReq
    {
        public Guid? OrderId { get; set; } = Guid.Empty;

        [StringLength(128)]
        public string? Address { get; set; }

        [StringLength(50)]
        public string? Ward { get; set; }

        [StringLength(30)]
        public string? District { get; set; }

        [StringLength(30)]
        public string? City { get; set; }
        public Guid[]? Details { get; set; } 
        public string? PaymentMethod { get; set; }
        [StringLength(11)]
        public string? PhoneNumber { get; set; }
        public string? FullName { get; set; }
        public string? NickName { get; set; }
        public bool IsSaveToken { get; set; } = false;

        public bool IsUseToken { get; set; } = false;
        public Decimal ShippingCost { get; set; }
    }
}
