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
    public class Message
    {
        [Key]
        public Guid MessageId { get; set; }
        public string UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        [ValidateNever]
        public ApplicationUser? User { get; set; }
        public string Content { get; set; }
        public DateTime SendAt { get; set; }
        public Guid ChatId { get; set; }

        public Chat? Chat { get; set; }
    }
}
