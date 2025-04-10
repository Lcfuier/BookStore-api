using AutoMapper;
using BookStore.Application.Interface;
using BookStore.Domain.DTOs;
using BookStore.Domain.Models;
using BookStore.Domain.Queries;
using BookStore.Domain.Result;
using BookStore.Infrastructure.Interface;
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
                Message = "Successful",
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
                    Message = "User not exist"
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
                    Message = "Cart is empty"
                };
            }
            var data= await _data.Cart.GetAsync(new QueryOptions<Cart>
            {
                Where= c=>c.UserId.Equals(user.Id)
            });
            
            data.CartItems = (await _cartItemService.GetAllCartItemsByCartIdAsync(data.CartId)).ToList();
            return new Result<GetCartRes>
            {
                Success = true,
                Data = _mapper.Map<GetCartRes>(data),
                Message = "Successful"
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
                result.Message = "User not exist";
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
                result.Message = "not found book";
                result.Data = null;
                return result;
            }
            if (existBook.Inventory < req.Quantity)
            {
                result.Success = false;
                result.Message = "Insufficient stock";
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
                    CartItemID = new Guid()
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
                        CartItemID = new Guid()
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
            result.Message = "Add item successful";
            result.Data = null;
            return result;
        }
        public async Task<Result<CartItem>> RemoveCartItemAsync(string userName, Guid ItemId)
        {
            Result<CartItem> result = new Result<CartItem>();
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                result.Success = false;
                result.Message = "User not exist";
                result.Data = null;
                return result;
            }
            CartItem item = await _data.CartItem.GetAsync(new QueryOptions<CartItem>
            {
                Where = c => c.CartItemID.Equals(ItemId)
            }) ?? throw new Exception("Item not found.");

            _data.CartItem.Remove(item);
            await _data.SaveAsync();
            result.Success = true;
            result.Message = "Successful";
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
                result.Message = "User not exist";
                result.Data = null;
                return result;
            }
            CartItem item = await _data.CartItem.GetAsync(new QueryOptions<CartItem>
            {
                Where = c => c.CartItemID.Equals(ItemId)
            }) ?? throw new Exception("Item not found.");
            item.Quantity += 1;
            _data.CartItem.Update(item);
            await _data.SaveAsync();
            result.Success = true;
            result.Message = "Successful";
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
                result.Message = "User not exist";
                result.Data = null;
                return result;
            }
            CartItem item = await _data.CartItem.GetAsync(new QueryOptions<CartItem>
            {
                Where = c => c.CartItemID.Equals(ItemId)
            }) ?? throw new Exception("Item not found.");
            if (item.Quantity == 1)
            {
                await _cartItemService.RemoveCartItemAsync(item);
            }
            else
            {
                item.Quantity -= 1;
                await _data.SaveAsync();
            }
            result.Success = true;
            result.Message = "Successful";
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
                result.Message = "User not exist";
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
