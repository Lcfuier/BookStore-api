using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Domain.DTOs
{
    public class UpdateUserReq
    {
        public bool isLockout {  get; set; }=false;
        public string? role { get; set; }
    }
}
