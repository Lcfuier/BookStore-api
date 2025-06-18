using BookStore.Application.InterfacesRepository;
using BookStore.Domain.Models;
using BookStore.Infrastructure.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Infrastructure.Repositorys
{
    public class AuthorRepository : Repository<Author>, IAuthorRepository
    {
        public AuthorRepository(ApplicationDbContext context) : base(context)
        {
        }

        public void Update(Author author)
        {
            _dbContext.Update(author);
        }

    }
}
