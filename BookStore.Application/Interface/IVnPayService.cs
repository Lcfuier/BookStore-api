using BookStore.Domain.Constants.VnPay;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Application.Interface
{
    public interface IVnPayService
    {
        Task<string> CreateTokenPaymentUrl(HttpContext httpContext, VnPayRequestModel req, string userName, string nickName);
        Task<VnPayResponeModel> PaymentExecuteAsync(IQueryCollection collection);
        string CreatePaymentUrl(HttpContext httpContext, VnPayRequestModel request, string userName, string? nickName);
    }
}
