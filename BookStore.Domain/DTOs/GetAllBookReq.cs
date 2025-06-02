using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Domain.DTOs
{
    public class GetAllBookReq
    {
        public int Page { get; set; }
        public int Size { get; set; }
        public string? term { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? AuthorId { get; set; }
        public Guid? PublisherId { get; set; }
        public string? OrderBy { get; set; }
        public Boolean? IsAsc { get; set; }=true;
    }
}
