using BookStore.Domain.DTOs;
using BookStore.Domain.Models;
using BookStore.Infrastructure.Data;
using BookStore.Application.InterfacesRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<List<RevenuePointRes>> GetRevenueByDate(DateFilter filter)
        {
            DateTime from = filter.FromDate?.ToUniversalTime() ?? DateTime.MinValue;
            DateTime to = filter.ToDate?.ToUniversalTime() ?? DateTime.UtcNow;
            var data = await _dbContext.Order
                .Where(o => o.CreatedTime >= from && o.CreatedTime <= to)
                .GroupBy(o => o.CreatedTime.Value.ToUniversalTime().Date)
                .Select(g => new RevenuePointRes
                {
                    Date = g.Key,
                    TotalRevenue = g.Sum(x => x.Total)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return data;
        }
        public async Task<List<BookSoldStatRes>> GetBooksSoldByDate(DateFilter filter)
        {
            DateTime from = filter.FromDate?.ToUniversalTime() ?? DateTime.MinValue;
            DateTime to = filter.ToDate?.ToUniversalTime() ?? DateTime.UtcNow;

            var data = await _dbContext.Order
                .Where(o => o.CreatedTime >= from && o.CreatedTime <= to)
                .SelectMany(o => o.OrderDetails!)
                .GroupBy(od => od.Order.CreatedTime!.Value.ToUniversalTime().Date)
                .Select(g => new BookSoldStatRes
                {
                    Date = g.Key,
                    TotalBooksSold = g.Sum(x => x.Quantity)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return data;

        }
        public async Task<List<Order>> ExportOrdersAsync(DateFilter filter)
        {
            DateTime from = filter.FromDate?.ToUniversalTime() ?? DateTime.MinValue;
            DateTime to = filter.ToDate?.ToUniversalTime() ?? DateTime.UtcNow;

            var data = await _dbContext.Order
                .Where(o => o.CreatedTime >= from && o.CreatedTime <= to)
                .OrderBy(o => o.CreatedTime)
                .ToListAsync();

            return data;

        }
    }
}
