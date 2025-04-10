using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Domain.DTOs
{
    public class UpdateAuthorReq
    {
        public Guid AuthorId { get; set; }
        public string AuthorFullName { get; set; }
    }
}
