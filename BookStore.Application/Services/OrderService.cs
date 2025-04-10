using BookStore.Application.Interface;
using BookStore.Domain.Constants;
using BookStore.Domain.Constants.VnPay;
using BookStore.Domain.DTOs;
using BookStore.Domain.Models;
using BookStore.Domain.Queries;
using BookStore.Domain.Result;
using BookStore.Infrastructure.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _data;
        private readonly UserManager<ApplicationUser> _userManager;
        public OrderService(IUnitOfWork data, UserManager<ApplicationUser> userManager)
        {
            _data = data;
            _userManager = userManager;
        }
        private async Task AddOrderAsync(Order order)
        {
            _data.Order.Add(order);
            await _data.SaveAsync();
        }
        public async Task<Result<IEnumerable<Order>>> GetOrderByUserNameAsync(string userId)
        {
            var result=new Result<IEnumerable<Order>>();
            QueryOptions<Order> options = new QueryOptions<Order>
            {
                Includes = "OrderDetails.Book",
                Where = mi => mi.UserId.Equals(userId)
            };
            var order = await _data.Order.ListAllAsync(options);
            result.Success = true;
            result.Data = order;
            return result;
        }
        public async Task<Result<Order>> Checkout(OrderReq req,string userName)
        {
            Result<Order> result = new Result<Order>();
            if(req.IsSaveToken && string.IsNullOrEmpty(req.NickName))
            {
                var paymentProfile = await _data.PaymentProfile.GetAsync(new QueryOptions<PaymentProfile>
                {
                    Where = c => c.User.UserName.Equals(userName) && c.Nickname.Equals(req.NickName)
                });
                if (paymentProfile != null)
                {
                    result.Success = false;
                    result.Message = "Nickname is exist";
                    result.Data = null;
                    return result;
                }
            }
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                result.Success = false;
                result.Message = "User not exist";
                result.Data = null;
                return result;
            }
            if (!req.OrderId.Equals(Guid.Empty))
            {
                result.Success = true;
                result.Message = "suceessful";
                result.Data= await _data.Order.GetAsync(new QueryOptions<Order>
                {
                    Where=c=>c.OrderId.Equals(req.OrderId),
                    Includes= "OrderDetails.Book"
                });
                return result;
            }
            Order order=new Order();
            order.User= user;
            order.Address= req.Address;
            order.Ward= req.Ward;
            order.OrderId = new Guid();
            order.OrderStatus = OrderStatus.StatusPending;
            order.District = req.District;
            order.City = req.City;
            order.CreatedBy=user.UserName;
            order.CreatedTime= DateTime.UtcNow;
            order.PhoneNumber= req.PhoneNumber;
            order.PaymentMethod = req.PaymentMethod;
            order.FullName=req.FullName;
            order.OrderDetails = new List<OrderItem>();
            decimal amount = 0;
            foreach(var item in req.Details)
            {
                var detail = await _data.CartItem.GetAsync(new QueryOptions<CartItem>
                {
                    Where = c => c.CartItemID.Equals(item),
                    Includes = "Book",
                });
                var orderDetail = new OrderItem();
                orderDetail.OrderId=order.OrderId;
                orderDetail.Price=(detail.Book.Price-detail.Book.Price*detail.Book.DiscountPercent)*detail.Quantity;
                orderDetail.Book=detail.Book;
                orderDetail.Quantity=detail.Quantity;
                orderDetail.OrderDetailId = new Guid();
                amount += orderDetail.Price;
                order.OrderDetails.Add(orderDetail);
                _data.CartItem.Remove(detail);
            }
            order.Total = amount;
            order.PaymentStatus=PaymentStatus.PaymentStatusPending;

            await AddOrderAsync(order);
            result.Success = true;
            result.Message = "Successfull";
            result.Data = order;
            return result;
        }
        public async Task<Result<Order>> VnPayCheckoutUpdate(Guid id, VnPayResponeModel respone,string userName)
        {
            Result<Order> result = new Result<Order>();
            QueryOptions<Order> options = new QueryOptions<Order>
            {
                Includes = "OrderDetails",
                Where = mi => mi.OrderId.Equals(id)
            };
            Order order = await _data.Order.GetAsync(options);
            if (order == null)
            {
                result.Success = false;
                result.Message = "Order not exist";
                result.Data=null;
                return result;
            }
            order.PaymentStatus= PaymentStatus.PaymentStatusApproved;
            order.OrderStatus = OrderStatus.StatusApproved;
            order.TransactionId = respone.TransactionId;
            order.PaymentTime = DateTime.UtcNow;
            order.ModifiedTime=DateTime.UtcNow;
            order.ModifiedBy = userName;
            _data.Order.Update(order);
            await _data.SaveAsync();
            result.Success = true;
            result.Message = "Successfull";
            result.Data = null;
            return result;
        }
    }
}
