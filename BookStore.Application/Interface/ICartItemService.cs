using BookStore.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Application.Interface
{
    public interface ICartItemService
    {
        Task<CartItem?> GetCartItemByIdAsync(Guid Id);
        Task<CartItem?> GetCartItemByCartIdAsync(Guid CartId);
        Task<IEnumerable<CartItem>> GetAllCartItemsByCartIdAsync(Guid CartId);
        Task<int?> GetQuantityAsync(Guid CartId);
        Task AddCartItemAsync(CartItem cartItem);
        Task UpdateCartItemAsync(CartItem cartItem);
        Task RemoveCartItemAsync(CartItem cartItem);
    }
}
