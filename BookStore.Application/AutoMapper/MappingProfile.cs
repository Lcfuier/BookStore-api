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

            CreateMap<Review, ReviewRes>()
                .ForMember(dest=>dest.Id,opt=>opt.MapFrom(src=>src.Id))
                .ForMember(dest=>dest.UserId,opt=>opt.MapFrom(src=>src.UserId))
                .ForMember(dest=>dest.BookId,opt=>opt.MapFrom(src=>src.BookId))
                .ForMember(dest=>dest.Comment,opt=>opt.MapFrom(src=>src.Comment))
                .ForMember(dest=>dest.Rating,opt=>opt.MapFrom(src=>src.Rating))
                .ForMember(dest=>dest.CreatedTime,opt=>opt.MapFrom(src=>src.CreatedTime))
                .ForMember(dest=>dest.userfullName,opt=>opt.MapFrom(src=>src.User.FullName));
            CreateMap<PaymentProfile, GetPaymentProfileRes>();
            CreateMap<GetPaymentProfileRes, PaymentProfile>();

            CreateMap<OrderItem, OrderItemRes>();
            CreateMap<OrderItemRes, OrderItem>();

            CreateMap<GetOrderByUserNameRes, Order>();
            CreateMap<Order, GetOrderByUserNameRes>()
                .ForMember(dest => dest.OrderDetails, opt => opt.MapFrom(src => src.OrderDetails));

            CreateMap<Order, GetOrderByIdRes>();

            CreateMap<ApplicationUser, GetAllUserRes>();
        }
    }
}
