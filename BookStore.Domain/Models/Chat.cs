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
    public class Chat
    {
        [Key]
        public Guid ChatId { get; set; }
        public string? UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        [ValidateNever]
        public ApplicationUser? User { get; set; }
        public string? LastSenderId { get; set; }

        //audit
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastMessageAt { get; set; }
        //
        public ICollection<Message>? Messages { get; set; }
    }
}
