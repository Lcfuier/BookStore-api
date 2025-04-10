using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Domain.DTOs
{
    public class UpdateCategoryReq
    {
        public Guid CategoryId { get; set; }
        public string Name { get; set; }
    }
}
