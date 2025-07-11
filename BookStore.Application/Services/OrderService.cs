﻿using AutoMapper;
using BookStore.Application.Interface;
using BookStore.Application.InterfacesRepository;
using BookStore.Domain.Constants;
using BookStore.Domain.Constants.VnPay;
using BookStore.Domain.DTOs;
using BookStore.Domain.Models;
using BookStore.Domain.Queries;
using BookStore.Domain.Result;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BookStore.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _data;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IGhnService _ghnService;
        private readonly IEncryptionService _encryptionService;
        public OrderService(IUnitOfWork data, UserManager<ApplicationUser> userManager,IMapper mapper, IGhnService ghnService, IEncryptionService encryptionService)
        {
            _data = data;
            _userManager = userManager;
            _mapper= mapper;  
            _ghnService= ghnService;
            _encryptionService= encryptionService;
        }
        private async Task AddOrderAsync(Order order)
        {
            _data.Order.Add(order);
            await _data.SaveAsync();
        }
        public async Task<Result<PaginationResponse<GetOrderByUserNameRes>>> GetOrderByUserNameAsync(int page,int size,string userName,string status,string orderCode)
        {
            var result=new Result<IEnumerable<GetOrderByUserNameRes>>();
            IEnumerable<GetOrderByUserNameRes> data;
            var user = await _userManager.FindByNameAsync(userName);
           
            QueryOptions<Order> options = new QueryOptions<Order>
            {
                Includes = "OrderDetails.Book",
                OrderBy = c => c.CreatedTime,
                OrderByDirection="desc"
            };
            var roles=await _userManager.GetRolesAsync(user);
            if (roles is null || (roles.FirstOrDefault()?.ToLower() != Roles.Admin.ToLower() && roles.FirstOrDefault()?.ToLower() != Roles.Librarian.ToLower()))
            {
                options.Where = mi => mi.UserId.Equals(user.Id);
            }
            if (orderCode != null)
            {
                options.Where = mi => mi.OrderCode.ToLower().Contains(orderCode.ToLower());
            }
            if (status != null)
            {
                options.Where = mi => mi.OrderStatus.ToLower().Equals(status.ToLower());
            }
            if (page < 1)
            {
                var exist= await _data.Order.ListAllAsync(options);
                data= _mapper.Map<IEnumerable<GetOrderByUserNameRes>>(exist);
            }
            else
            {
                options.PageNumber = page;
                options.PageSize = size;
                var exist = await _data.Order.ListAllAsync(options);
                data = _mapper.Map<IEnumerable<GetOrderByUserNameRes>>(exist);
            }
            PaginationResponse<GetOrderByUserNameRes> paginationResponse = new PaginationResponse<GetOrderByUserNameRes>
            {
                PageNumber = page,
                PageSize = size,
                // must be above the TotalRecords bc it has multiple Where clauses
                Items = data,
                TotalRecords = await _data.Order.CountAsync()
            };
            return new Result<PaginationResponse<GetOrderByUserNameRes>>
            {
                Data = paginationResponse,
                Message = "Successful",
                Success = true
            };
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
                    result.Message = "Tên gợi nhớ đã tồn tại!";
                    result.Data = null;
                    return result;
                }
            }
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                result.Success = false;
                result.Message = "Người dùng không tồn tại!";
                result.Data = null;
                return result;
            }
            if (!req.OrderId.Equals(Guid.Empty))
            {
                result.Success = true;
                result.Message = "Đặt hàng thành công!";
                result.Data= await _data.Order.GetAsync(new QueryOptions<Order>
                {
                    Where=c=>c.OrderId.Equals(req.OrderId),
                    Includes= "OrderDetails.Book"
                });
                return result;
            }
            Order order=new Order();
            order.User= user;
            order.Address= _encryptionService.Encrypt(req.Address);
            order.Ward= _encryptionService.Encrypt(req.Ward);
            order.OrderId = new Guid();
            order.OrderStatus = OrderStatus.StatusPending;
            order.District = _encryptionService.Encrypt(req.District);
            order.City = _encryptionService.Encrypt(req.City);
            order.CreatedBy=user.UserName;
            order.CreatedTime= DateTime.UtcNow;
            order.PhoneNumber= _encryptionService.Encrypt(req.PhoneNumber);
            order.FullName= _encryptionService.Encrypt(req.FullName);
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
                var book = detail.Book;

                if (book.Inventory < detail.Quantity)
                {
                    result.Success = false;
                    result.Message = "Số lượng sách trong kho không đủ";
                    result.Data = null;
                    return result;
                }
                book.Inventory -= detail.Quantity;
                _data.Book.Update(book);
                _data.CartItem.Remove(detail);
            }
            order.ShippingCost = (await _ghnService.CalculateShippingFeeAsync(req.shippingFeeRequest)).ToString();
            order.Total = amount + Decimal.Parse(order.ShippingCost);
            if (req.PaymentMethod.ToLower()==PaymentMethod.PaymentMethodCash.ToLower())
            {
                order.PaymentStatus = PaymentStatus.PaymentStatusCash;
                order.OrderStatus = OrderStatus.StatusApproved;
            }
            else
            {
                
                order.PaymentStatus = PaymentStatus.PaymentStatusPending;
            }
            order.PaymentMethod = req.PaymentMethod;
            order.OrderCode = GenerateOrderCode();
            await AddOrderAsync(order);
            result.Success = true;
            result.Message = "Đặt hàng thành công";
            result.Data = order;
            return result;
        }
        public async Task<Result<string>> VnPayCheckoutUpdate(Guid id, VnPayResponeModel respone,string userName)
        {
            Result<string> result = new Result<string>();
            QueryOptions<Order> options = new QueryOptions<Order>
            {
                Includes = "OrderDetails",
                Where = mi => mi.OrderId.Equals(id)
            };
            Order order = await _data.Order.GetAsync(options);
            if (order == null)
            {
                result.Success = false;
                result.Message = "Đơn hàng không tồn tại!";
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
            result.Message = "Thanh toán thành công!";
            result.Data = order.OrderId.ToString();
            return result;
        }
        public async Task<Result<GetOrderByIdRes>> GetOrderByIdAsync(Guid id,string username)
        {
            var result = new Result<GetOrderByIdRes>();
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return new Result<GetOrderByIdRes>
                {
                    Success = false,
                    Data = null,
                    Message = "Người dùng không tồn tại !"
                };
            }
            var data = new Order();
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.FirstOrDefault()?.ToLower() == Roles.Admin.ToLower() || roles.FirstOrDefault()?.ToLower() == Roles.Librarian.ToLower())
            {
                data = await _data.Order.GetAsync(new QueryOptions<Order>()
                {
                    Where = c => c.OrderId.Equals(id) ,
                    Includes = "OrderDetails.Book"
                });
            }
            else
            {
                data = await _data.Order.GetAsync(new QueryOptions<Order>()
                {
                    Where = c => c.OrderId.Equals(id) && c.UserId.Equals(user.Id),
                    Includes = "OrderDetails.Book"
                });
            }
            if(data is null)
            {
                result.Data = null;
                result.Message = "Không tìm thấy đơn hàng";
                result.Success = false;
                return result;
            }
            result.Data=_mapper.Map<GetOrderByIdRes>(data);
            result.Success=true;
            return result;
        }
        public async Task<Result<GetOrderByIdRes>> UpdateOrderAsyc(string username,UpdateOrderReq req,Guid id)
        {
            var result = new Result<GetOrderByIdRes>();
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return new Result<GetOrderByIdRes>
                {
                    Success = false,
                    Data = null,
                    Message = "Người dùng không tồn tại !"
                };
            }
            var data = new Order();
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.FirstOrDefault()?.ToLower() == Roles.Admin.ToLower() || roles.FirstOrDefault()?.ToLower() == Roles.Librarian.ToLower())
            {
                data = await _data.Order.GetAsync(new QueryOptions<Order>()
                {
                    Where = c => c.OrderId.Equals(id),
                    Includes = "OrderDetails.Book"
                });
            }
            else
            {
                data = await _data.Order.GetAsync(new QueryOptions<Order>()
                {
                    Where = c => c.OrderId.Equals(id) && c.UserId.Equals(user.Id),
                    Includes = "OrderDetails.Book"
                });
            }
            if (data is null)
            {
                result.Data = null;
                result.Message = "Không tìm thấy đơn hàng";
                result.Success = false;
                return result;
            }
            data.ModifiedTime = DateTime.UtcNow;
            data.ModifiedBy = user.UserName;
            if (req.OrderStatus.Equals(OrderStatus.StatusInProcess))
            {
                if (roles is null || (roles.FirstOrDefault()?.ToLower() != Roles.Admin.ToLower() && roles.FirstOrDefault()?.ToLower() != Roles.Librarian.ToLower()))
                {
                    result.Data = null;
                    result.Message = "Lỗi khi thực hiện!";
                    result.Success = false;
                    return result;
                }
                else
                {
                    data.OrderStatus = OrderStatus.StatusInProcess;
                    _data.Order.Update(data);
                    await _data.SaveAsync();
                    result.Message = "Đã chuyển sang xử lí!";
                    result.Success = true;
                    return result;
                }
            }
            else if (req.OrderStatus.Equals(OrderStatus.StatusShipped))
            {
                if (roles is null || (roles.FirstOrDefault()?.ToLower() != Roles.Admin.ToLower() && roles.FirstOrDefault()?.ToLower() != Roles.Librarian.ToLower()))
                {
                    result.Data = null;
                    result.Message = "Lỗi khi thực hiện!";
                    result.Success = false;
                    return result;
                }
                else
                {
                    if (string.IsNullOrEmpty(req.Carrier) && string.IsNullOrEmpty(req.TrackingNumber))
                    {
                        result.Data = null;
                        result.Message = "Vui lòng nhập mã vận chuyện và mã theo dõi!";
                        result.Success = false;
                        return result;
                    }
                    else
                    {
                        data.OrderStatus = OrderStatus.StatusShipped;
                        data.TrackingNumber = req.TrackingNumber;
                        data.Carrier = req.Carrier;
                        _data.Order.Update(data);
                        await _data.SaveAsync();
                        result.Message = "Đã chuyển giao cho đơn vị vận chuyển!";
                        result.Success = true;
                        return result;
                    }
                }
            }
            else if (req.OrderStatus.Equals(OrderStatus.StatusCompleted))
            {
                if (roles.FirstOrDefault()?.ToLower() != Roles.User.ToLower())
                {
                    result.Data = null;
                    result.Message = "Lỗi khi thực hiện!";
                    result.Success = false;
                    return result;
                }
                else
                {
                    data.OrderStatus = OrderStatus.StatusCompleted;
                    _data.Order.Update(data);
                    await _data.SaveAsync();
                    result.Message = "Đơn hàng đã hoàn tất!";
                    result.Success = true;
                    return result;
                }
            }
            else if (req.OrderStatus == OrderStatus.StatusCancelled)
            {
                data.OrderStatus = OrderStatus.StatusCancelled;
                _data.Order.Update(data);
                await _data.SaveAsync();
                result.Message = "Hủy đơn hàng thành công!";
                result.Success = true;
                return result;
            }

            result.Success = true;
            return result;
        }
        public async Task<List<RevenuePointRes>> GetRevenueByDate(DateFilter filter,string userName)
        {
            List<RevenuePointRes> result= new List<RevenuePointRes> ();
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return result;
            }
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.FirstOrDefault()?.ToLower() != Roles.Admin.ToLower() && roles.FirstOrDefault()?.ToLower() != Roles.Librarian.ToLower())
            {
                return result;
            }
            return await _data.Order.GetRevenueByDate(filter);
        }
        public async Task<List<BookSoldStatRes>> GetBooksSoldByDate(DateFilter filter, string userName)
        {
            List <BookSoldStatRes> result = new List<BookSoldStatRes>();
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return result;
            }
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.FirstOrDefault()?.ToLower() != Roles.Admin.ToLower() && roles.FirstOrDefault()?.ToLower() != Roles.Librarian.ToLower())
            {
                return result;
            }
            return await _data.Order.GetBooksSoldByDate(filter);
        }
        public async Task<FileContentResult> ExportOrdersAsync(DateFilter filter, string userName)
        {
            List<BookSoldStatRes> result = new List<BookSoldStatRes>();
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return null;
            }
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.FirstOrDefault()?.ToLower() != Roles.Admin.ToLower() && roles.FirstOrDefault()?.ToLower() != Roles.Librarian.ToLower())
            {
                return null;
            }
            var data=await _data.Order.ExportOrdersAsync(filter);
            var orders = _mapper.Map<List<GetOrderByUserNameRes>>(data);
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Orders");

            // Header
            worksheet.Cell(1, 1).Value = "STT";
            worksheet.Cell(1, 2).Value = "Mã đơn";
            worksheet.Cell(1, 3).Value = "Khách hàng";
            worksheet.Cell(1, 4).Value = "SĐT";
            worksheet.Cell(1, 5).Value = "Địa chỉ";
            worksheet.Cell(1, 6).Value = "Ngày đặt";
            worksheet.Cell(1, 7).Value = "Trạng thái";
            worksheet.Cell(1, 8).Value = "Thanh toán";
            worksheet.Cell(1, 9).Value = "Mã giao dịch";
            worksheet.Cell(1, 10).Value = "Tổng tiền";

            // Ghi dữ liệu
            for (int i = 0; i < orders.Count; i++)
            {
                var o = orders[i];
                var row = i + 2;

                worksheet.Cell(row, 1).Value = i + 1;
                worksheet.Cell(row, 2).Value = o.OrderCode;
                worksheet.Cell(row, 3).Value = o.FullName;
                worksheet.Cell(row, 4).Value = o.PhoneNumber;
                worksheet.Cell(row, 5).Value = o.City+", "+o.District+", "+o.Ward+", "+o.Address;
                worksheet.Cell(row, 6).Value = o.CreatedTime?.ToString("dd/MM/yyyy HH:mm");
                worksheet.Cell(row, 7).Value = o.OrderStatus;
                worksheet.Cell(row, 8).Value = o.PaymentMethod;
                worksheet.Cell(row, 9).Value = o.TransactionId;
                worksheet.Cell(row, 10).Value = o.Total;
            }

            // Tổng doanh thu ở dòng cuối
            int totalRow = orders.Count + 2;
            worksheet.Cell(totalRow, 9).Value = "Tổng doanh thu:";
            worksheet.Cell(totalRow, 10).FormulaA1 = $"=SUM(J2:J{orders.Count + 1})";
            worksheet.Cell(totalRow, 9).Style.Font.Bold = true;
            worksheet.Cell(totalRow, 10).Style.Font.Bold = true;

            // Format cột số tiền
            worksheet.Column(10).Style.NumberFormat.Format = "#,##0 đ";
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"DonHang_{filter.FromDate}-{filter.ToDate}.xlsx";
            return new FileContentResult(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = fileName
            };
        }
        private static string GenerateOrderCode()
        {
            string prefix = "ORD";
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

            // Tạo chuỗi ngẫu nhiên gồm 4 ký tự chữ hoặc số
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var randomStr = new string(Enumerable.Range(0, 4)
                .Select(_ => chars[random.Next(chars.Length)]).ToArray());

            return $"{prefix}{timestamp}{randomStr}";
        }
    }
}
