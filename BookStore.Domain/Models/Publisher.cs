using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BookStore.Domain.Models
{
    public class Publisher
    {
        [Key]
        public Guid PublisherId { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        //audit
        public DateTime? CreatedTime { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedTime { get; set; }
        public string? ModifiedBy { get; set; }
        //
        [JsonIgnore]
        public ICollection<Book>? Books { get; set; }
    }
}
