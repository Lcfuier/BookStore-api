﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace BookStore.Domain.Models
{
    public class CartItem
    {
        [Key]
        public Guid CartItemID { get; set; }
        [Required]
        [Range(1, 1000)]
        public int Quantity { get; set; }

        public Guid BookId { get; set; }
        public Book? Book { get; set; }

        public Guid CartId { get; set; }
        [JsonIgnore]
        public Cart? Cart { get; set; }

        public DateTime? ModifiedTime { get; set; }
    }
}
