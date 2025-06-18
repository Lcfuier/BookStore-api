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
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public void Update(Category category)
        {
            _dbContext.Update(category);
        }
    }
}
