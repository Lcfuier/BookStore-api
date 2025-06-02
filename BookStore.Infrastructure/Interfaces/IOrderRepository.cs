using BookStore.Domain.DTOs;
using BookStore.Domain.Models;
using BookStore.Infrastructure.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Infrastructure.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        void Update(Order order);
        Task<List<BookSoldStatRes>> GetBooksSoldByDate(DateFilter filter);
        Task<List<RevenuePointRes>> GetRevenueByDate(DateFilter filter);
    }
}
