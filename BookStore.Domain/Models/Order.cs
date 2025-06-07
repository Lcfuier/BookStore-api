using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace BookStore.Domain.Models
{
    public class Order
    {
        [Key]
        public Guid OrderId { get; set; }
        public Decimal Total { get; set; }
        public string OrderStatus { get; set; }
        [StringLength(256)]
        [Unicode(false)]
        public string? TransactionId { get; set; }

        public string? Address { get; set; }

        public string? Ward { get; set; }

        public string? District { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FullName { get; set; }

        public string? City { get; set; }
        [BindNever]
        public ICollection<OrderItem>? OrderDetails { get; set; }
        [StringLength(128)]
        [Unicode(false)]
        public string? TrackingNumber { get; set; }
        public string? Carrier { get; set; }
        public string? ShippingCost { get; set; }
        [StringLength(20)]
        [Unicode(false)]
        public string? PaymentStatus { get; set; }
        public string? OrderCode { get; set; }  
        public string? PaymentMethod { get; set; }
        public DateTime? PaymentTime { get; set; }
        //audit
        public DateTime? CreatedTime { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedTime { get; set; }
        public string? ModifiedBy { get; set; }
        //
        public string UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        [ValidateNever]
        public ApplicationUser? User { get; set; }
    }

}
