using BookStore.Domain.DTOs;
using BookStore.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Application.InterfacesRepository
{
    public interface IOrderRepository : IRepository<Order>
    {
        void Update(Order order);
        Task<List<BookSoldStatRes>> GetBooksSoldByDate(DateFilter filter);
        Task<List<RevenuePointRes>> GetRevenueByDate(DateFilter filter);
        Task<List<Order>> ExportOrdersAsync(DateFilter filter);
    }
}
