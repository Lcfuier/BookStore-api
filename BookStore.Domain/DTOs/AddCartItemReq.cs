using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Domain.DTOs
{
    public class AddCartItemReq
    {
        public Guid BookId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Vui lòng nhập số nguyên lớn hơn 0 cho Số lượng.")]
        [Range(1, 1000, ErrorMessage = "Vui lòng nhập số nguyên lớn hơn 0 cho Số lượng.")]
        public int Quantity { get; set; }
    }
}
