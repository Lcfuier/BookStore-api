﻿using BookStore.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Application.InterfacesRepository
{
    public interface IAuthorRepository : IRepository<Author>
    {
        void Update(Author author);
    }
}
