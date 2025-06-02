using BookStore.Domain.DTOs;
using BookStore.Domain.Models;
using BookStore.Domain.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Application.Interface
{
    public interface IPaymentProfileService
    {
        Task<Result<IEnumerable<GetPaymentProfileRes>>> GetAllPaymentProfileAsync(string userName);
        Task<Result<GetPaymentProfileRes>> GetPaymentProfileByNameAsync(string userName, string name);
        Task<Result<PaymentProfile>> RemovePaymentProfileAsync(Guid id);
    }
}
