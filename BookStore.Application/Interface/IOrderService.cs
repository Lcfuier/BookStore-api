using BookStore.Domain.Constants.VnPay;
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
    public interface IOrderService
    {
        Task<Result<IEnumerable<Order>>> GetOrderByUserNameAsync(string userId);
        Task<Result<Order>> Checkout(OrderReq req, string userName);
        Task<Result<Order>> VnPayCheckoutUpdate(Guid id, VnPayResponeModel respone, string userName);

    }
}
