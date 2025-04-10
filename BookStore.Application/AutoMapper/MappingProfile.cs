using AutoMapper;
using BookStore.Domain.DTOs;
using BookStore.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Application.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {

            CreateMap<Book, AddBookReq>();
            CreateMap<AddBookReq, Book>();
            CreateMap<Book, UpdateBookReq>();
            CreateMap<UpdateBookReq, Book>();

            CreateMap<Cart,GetCartRes>();
            CreateMap<GetCartRes,Cart>();   

        }
    }
}
