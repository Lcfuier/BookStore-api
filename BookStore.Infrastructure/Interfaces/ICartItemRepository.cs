using BookStore.Domain.Models;
using BookStore.Infrastructure.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Infrastructure.Interfaces
{
    public interface ICartItemRepository : IRepository<CartItem>
    {
        void Update(CartItem cartItem);
    }
}
