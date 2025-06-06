using BookStore.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Application.Interface
{
    public interface IGhnService
    {
        Task<int> CalculateShippingFeeAsync(ShippingFeeRequest requestModel);
    }
}
