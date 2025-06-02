using BookStore.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Infrastructure.Interface
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository User { get; }
        IAuthorRepository Author { get; }
        IPublisherRepository Publisher { get; }
        ICategoryRepository Category { get; }
        IBookRepository Book { get; }
        ICartRepository Cart { get; }
        ICartItemRepository CartItem { get; }
        IPaymentProfileRepository PaymentProfile { get; }
        IOrderDetailRepository OrderDetail { get; }
        IOrderRepository Order { get; }
        IReviewRepository Review { get; }
        IChatRepository Chat { get; }
        IMessageRepository Message { get; }
        Task SaveAsync();
    }
}
