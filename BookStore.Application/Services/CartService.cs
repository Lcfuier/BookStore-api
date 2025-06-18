using AutoMapper;
using BookStore.Application.Interface;
using BookStore.Application.InterfacesRepository;
using BookStore.Domain.Constants;
using BookStore.Domain.DTOs;
using BookStore.Domain.Models;
using BookStore.Domain.Queries;
using BookStore.Domain.Result;
using Google.Apis.Drive.v3.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Application.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _data;
        private readonly ICartItemService _cartItemService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        public CartService(IUnitOfWork data, ICartItemService cartItemService, UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _data = data;
            _cartItemService = cartItemService;
            _userManager = userManager;
            _mapper = mapper;
        }
        public async Task<Result<IEnumerable<Cart>>> GetAllCartsAsync()
        {
            IEnumerable<Cart> data;
            QueryOptions<Cart> options = new QueryOptions<Cart>
            {
            };
            data = await _data.Cart.ListAllAsync(options);
            return new Result<IEnumerable<Cart>>
            {
                Data = data,
                Success = true
            };
        }
        public async Task<Result<GetCartRes>> GetCartByUserAsync(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return new Result<GetCartRes>
                {
                    Success = true,
                    Data = null,
                    Message = "Người dùng không tồn tại!"
            };
            }
            Cart? cart = await _data.Cart.GetAsync(new QueryOptions<Cart>
            {
                Where = c => c.UserId.Equals(user.Id)
            });

            if (cart is null)
            {
                cart = new Cart()
                {
                    UserId = user.Id,
                    CartId = new Guid()
                };
                await AddCartAsync(cart);
                return new Result<GetCartRes>
                {
                    Success = true,
                    Data = null,
                    Message = "Giỏ hàng trống"
                };
            }
            var data= await _data.Cart.GetAsync(new QueryOptions<Cart>
            {
                Where= c=>c.UserId.Equals(user.Id)
            });
            
            var item = (await _cartItemService.GetAllCartItemsByCartIdAsync(data.CartId)).ToList();
            data.CartItems=item.OrderByDescending(c=>c.ModifiedTime).ToList();
            return new Result<GetCartRes>
            {
                Success = true,
                Data = _mapper.Map<GetCartRes>(data),
            };

        }
        public async Task AddCartAsync(Cart cart)
        {
            _data.Cart.Add(cart);
            await _data.SaveAsync();
        }
        public async Task<Result<CartItem>> AddCartItemAsync(string userName, AddCartItemReq req)
        {
            Result<CartItem> result = new Result<CartItem>();
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                result.Success = false;
                result.Message = "Người dùng không tồn tại";
                result.Data = null;
                return result;
            }
            Book? existBook = await _data.Book.GetAsync(new QueryOptions<Book>
            {
                Where = ci => ci.BookId.Equals(req.BookId)
            });
            if (existBook is null)
            {
                result.Success = false;
                result.Message = "Không tìm thấy sách!";
                result.Data = null;
                return result;
            }
            if (existBook.Inventory < req.Quantity)
            {
                result.Success = false;
                result.Message = "Không đủ số lượng trong kho!";
                result.Data = null;
                return result;
            }
            Cart? cart = await _data.Cart.GetAsync(new QueryOptions<Cart>
            {
                Where = c => c.UserId.Equals(user.Id)
            });

            if (cart is null)
            {
                cart = new Cart()
                {
                    UserId = user.Id,
                    CartId = new Guid()
                };
                await AddCartAsync(cart);
                CartItem item = new CartItem()
                {
                    BookId = req.BookId,
                    Quantity = req.Quantity,
                    CartId = cart.CartId,
                    CartItemID = new Guid(),
                    ModifiedTime= DateTime.UtcNow,  
                };
                _data.CartItem.Add(item);
            }
            else
            {
                var item = await _data.CartItem.GetAsync(new QueryOptions<CartItem>
                {
                    Where=c=>c.CartId.Equals(cart.CartId) && c.BookId.Equals(existBook.BookId)
                });
                if (item is null)
                {
                    item = new CartItem()
                    {
                        BookId = req.BookId,
                        Quantity = req.Quantity,
                        CartId = cart.CartId,
                        CartItemID = new Guid(),
                        ModifiedTime = DateTime.UtcNow,
                    };
                    _data.CartItem.Add(item);
                }
                else
                {
                    if (item.Quantity > 0)
                    {
                        item.Quantity += req.Quantity;
                        _data.CartItem.Update(item);
                    }
                    else

                        _data.CartItem.Remove(item);
                }

            }
            await _data.SaveAsync();
            result.Success = true;
            result.Message = "Thêm vào giỏ hàng thánh công!";
            result.Data = null;
            return result;
        }
        public async Task<Result<CartItem>> RemoveCartItemAsync(string userName, Guid ItemId)
        {
            Result<CartItem> result = new Result<CartItem>();
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return new Result<CartItem>
                {
                    Success = false,
                    Data = null,
                    Message = "Người dùng không tồn tại !"
                };
            }
            CartItem item = await _data.CartItem.GetAsync(new QueryOptions<CartItem>
            {
                Where = c => c.CartItemID.Equals(ItemId)
            }) ?? throw new Exception("Không tìm thấy sách");

            _data.CartItem.Remove(item);
            await _data.SaveAsync();
            result.Success = true;
            result.Message = "Xóa thành công";
            result.Data= null;
            return result;
        }
        public async Task<Result<CartItem>> Plus(string userName, Guid ItemId)
        {
            Result<CartItem> result = new Result<CartItem>();
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                result.Success = false;
                result.Message = "Người dùng không tồn tại!";
                result.Data = null;
                return result;
            }
            CartItem item = await _data.CartItem.GetAsync(new QueryOptions<CartItem>
            {
                Where = c => c.CartItemID.Equals(ItemId)
            }) ?? throw new Exception("Không tìm thấy sách");
            item.Quantity += 1;
            item.ModifiedTime= DateTime.UtcNow;
            _data.CartItem.Update(item);
            await _data.SaveAsync();
            result.Success = true;
            result.Message = "Cập nhật thành công";
            result.Data = null;
            return result;
        }
        public async Task<Result<CartItem>> Minus(string userName, Guid ItemId)
        {
            Result<CartItem> result = new Result<CartItem>();
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                result.Success = false;
                result.Message = "Người dùng không tồn tại!";
                result.Data = null;
                return result;
            }
            CartItem item = await _data.CartItem.GetAsync(new QueryOptions<CartItem>
            {
                Where = c => c.CartItemID.Equals(ItemId)
            }) ?? throw new Exception("Không tìm thấy sách!");
            if (item.Quantity == 1)
            {
                await _cartItemService.RemoveCartItemAsync(item);
            }
            else
            {
                item.Quantity -= 1;
                item.ModifiedTime = DateTime.UtcNow;
                await _data.SaveAsync();
            }
            result.Success = true;
            result.Message = "Cập nhật thành công";
            result.Data = null;
            return result;
        }
        public async Task<Result<int>> GetTotalCartItemsCountAsync(string userName)
        {
            Result<int> result = new Result<int>();
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                result.Success = false;
                result.Message = "Không tìm thấy người dùng!";
                result.Data = 0;
                return result;
            }
            Cart? cart = await _data.Cart.GetAsync(new QueryOptions<Cart>
            {
                Where = c => c.UserId.Equals(user.Id),
                Includes= "CartItems"
            });
            if(cart is null)
            {
                result.Success = true;
                result.Message = "";
                result.Data = 0;
                return result;
            }
            int count = 0;
            foreach (CartItem cartItem in cart.CartItems)
            {
                count += cartItem.Quantity;
            }
            result.Data = count;
            result.Success = true;
            return result;
        }
    }
    }
