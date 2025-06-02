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
    public class MessageRepository : Repository<Message>, IMessageRepository
    {
        public MessageRepository(ApplicationDbContext context) : base(context)
        {
        }

        public void Update(Message message)
        {
            _dbContext.Update(message);
        }
    }
}
