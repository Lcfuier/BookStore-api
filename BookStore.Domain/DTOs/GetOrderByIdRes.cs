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
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BookStore.Domain.DTOs
{
    public class GetOrderByIdRes
    {
        public Guid OrderId { get; set; }
        public Decimal Total { get; set; }
        public string OrderStatus { get; set; }
        [StringLength(256)]
        [Unicode(false)]
        public string? TransactionId { get; set; }

        [StringLength(128)]
        public string? Address { get; set; }

        [StringLength(50)]
        public string? Ward { get; set; }

        [StringLength(30)]
        public string? District { get; set; }
        [StringLength(11)]
        public string? PhoneNumber { get; set; }
        public string? FullName { get; set; }

        [StringLength(30)]
        public string? City { get; set; }
        [BindNever]
        public ICollection<OrderItemRes>? OrderDetails { get; set; } =new List<OrderItemRes>();   
        [StringLength(128)]
        [Unicode(false)]
        public string? TrackingNumber { get; set; }
        public string? Carrier { get; set; }
        public string? ShippingStatus { get; set; }
        public string? ShippingCost { get; set; }
        [StringLength(20)]
        [Unicode(false)]
        public string? PaymentStatus { get; set; }
        public string? PaymentMethod { get; set; }
        public string? OrderCode { get; set; }
        public DateTime? PaymentTime { get; set; }
        //audit
        public DateTime? CreatedTime { get; set; }
        public string? CreatedBy { get; set; }
        //
    }
}
