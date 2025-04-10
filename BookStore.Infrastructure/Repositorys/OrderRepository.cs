using BookStore.Domain.Models;
using BookStore.Infrastructure.Data;
using BookStore.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Infrastructure.Repositorys
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public void Update(Order order)
        {
            _dbContext.Update(order);
        }
    }
}
