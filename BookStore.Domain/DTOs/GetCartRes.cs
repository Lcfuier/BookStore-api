using BookStore.Domain.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Domain.DTOs
{
    public class GetCartRes
    {
        public Guid CartId { get; set; }
        public string UserId { get; set; }
        public ICollection<CartItem>? CartItems { get; set; }
    }
}
