using BookStore.Domain.Models;
using BookStore.Infrastructure.Interface;
using BookStore.Infrastructure.Repositorys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Infrastructure.Interfaces
{
    public interface IReviewRepository : IRepository<Review>
    {
        void Update(Review review);
    }
}
