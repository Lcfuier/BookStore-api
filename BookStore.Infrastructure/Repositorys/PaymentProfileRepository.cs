using BookStore.Domain.Models;
using BookStore.Infrastructure.Data;
using BookStore.Application.InterfacesRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Infrastructure.Repositorys
{
    public class PaymentProfileRepository : Repository<PaymentProfile>, IPaymentProfileRepository
    {
        public PaymentProfileRepository(ApplicationDbContext context) : base(context)
        {
        }

        public void Update(PaymentProfile paymentProfile)
        {
            _dbContext.Update(paymentProfile);
        }

    }
}
