using AutoMapper;
using BookStore.Infrastructure.Data;
using BookStore.Infrastructure.Interface;
using BookStore.Infrastructure.Interfaces;
using BookStore.Infrastructure.Repositorys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public IUserRepository User { get; private set; }
        public IAuthorRepository Author { get; private set; }
        public IPublisherRepository Publisher { get; private set; }
        public ICategoryRepository Category { get; private set; }
        public IBookRepository Book { get; private set; }
        public ICartRepository Cart { get; private set; }
        public ICartItemRepository CartItem { get; private set; }
        public IPaymentProfileRepository PaymentProfile { get; private set; }
        public IOrderDetailRepository OrderDetail { get; private set; }
        public IOrderRepository Order { get; private set; }
        public IReviewRepository Review { get; private set; }
        public IChatRepository Chat { get; private set; }
        public IMessageRepository Message { get; private set; }
        public UnitOfWork(ApplicationDbContext context, IMapper _mapper)
        {
            _context = context;
            User = new UserRepository(_context);
            Author = new AuthorRepository(_context);
            Publisher = new PublisherRepository(_context);
            Category = new CategoryRepository(_context);
            Cart = new CartRepository(_context);
            CartItem= new CartItemRepository(_context);
            Book = new BookRepository(_context, _mapper);
            PaymentProfile= new PaymentProfileRepository(_context);
            OrderDetail= new OrderDetailRepository(_context);
            Order= new OrderRepository(_context);
            Review= new ReviewRepository(_context);
            Chat= new ChatRepository(_context);
            Message = new MessageRepository(_context);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
