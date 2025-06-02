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
    public class ChatRepository : Repository<Chat>, IChatRepository
    {
        public ChatRepository(ApplicationDbContext context) : base(context)
        {
        }

        public void Update(Chat chat)
        {
            _dbContext.Update(chat);
        }
    }
}
