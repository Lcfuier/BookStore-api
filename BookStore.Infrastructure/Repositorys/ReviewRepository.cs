using BookStore.Domain.Models;
using BookStore.Infrastructure.Data;
using BookStore.Application.InterfacesRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Infrastructure.Repositorys
{
    public class ReviewRepository : Repository<Review>, IReviewRepository
    {
        public ReviewRepository(ApplicationDbContext context) : base(context)
        {
        }

        public void Update(Review review)
        {
            _dbContext.Update(review);
        }
    }
}
