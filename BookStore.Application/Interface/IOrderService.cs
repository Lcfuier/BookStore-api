using BookStore.Domain.Constants.VnPay;
using BookStore.Domain.DTOs;
using BookStore.Domain.Models;
using BookStore.Domain.Queries;
using BookStore.Domain.Result;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Application.Interface
{
    public interface IOrderService
    {
        Task<Result<PaginationResponse<GetOrderByUserNameRes>>> GetOrderByUserNameAsync(int page, int size, string userName,string status,string orderCode);
        Task<Result<Order>> Checkout(OrderReq req, string userName);
        Task<Result<string>> VnPayCheckoutUpdate(Guid id, VnPayResponeModel respone, string userName);
        Task<Result<GetOrderByIdRes>> GetOrderByIdAsync(Guid id, string username);
        Task<Result<GetOrderByIdRes>> UpdateOrderAsyc(string username, UpdateOrderReq req,Guid id);
        Task<List<BookSoldStatRes>> GetBooksSoldByDate(DateFilter filter,string userName);
        Task<List<RevenuePointRes>> GetRevenueByDate(DateFilter filter, string userName);
        Task<FileContentResult> ExportOrdersAsync(DateFilter filter, string userName);
    }
}
