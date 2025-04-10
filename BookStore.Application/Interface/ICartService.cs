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
    public interface ICartService
    {
        Task<Result<IEnumerable<Cart>>> GetAllCartsAsync();
        Task<Result<GetCartRes>> GetCartByUserAsync(string userName);
        Task AddCartAsync(Cart cart);
        Task<Result<CartItem>> AddCartItemAsync(string userName, AddCartItemReq req);
        Task<Result<CartItem>> RemoveCartItemAsync(string userName, Guid ItemId);
        Task<Result<CartItem>> Plus(string userName, Guid ItemId);
        Task<Result<CartItem>> Minus(string userName, Guid ItemId);
        Task<Result<int>> GetTotalCartItemsCountAsync(string userName);
    }
}
