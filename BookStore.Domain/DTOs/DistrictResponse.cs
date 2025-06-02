using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Domain.DTOs
{
    public class DistrictResponse
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public List<District> Data { get; set; }
    }
}
