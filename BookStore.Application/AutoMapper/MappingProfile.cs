using AutoMapper;
using BookStore.Application.Interface;
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
        public MappingProfile(IEncryptionService encryptionService)
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
                .ForMember(dest => dest.OrderDetails, opt => opt.MapFrom(src => src.OrderDetails))
                 .AfterMap((src, dest) =>
                  {
                      dest.FullName = encryptionService.Decrypt(src.FullName);
                      dest.PhoneNumber = encryptionService.Decrypt(src.PhoneNumber);
                      dest.Address = encryptionService.Decrypt(src.Address);
                      dest.Ward = encryptionService.Decrypt(src.Ward);
                      dest.District = encryptionService.Decrypt(src.District);
                      dest.City = encryptionService.Decrypt(src.City);
                  });
            CreateMap<Order, GetOrderByIdRes>()
                .AfterMap((src, dest) =>
                {
                    dest.FullName = encryptionService.Decrypt(src.FullName);
                    dest.PhoneNumber = encryptionService.Decrypt(src.PhoneNumber);
                    dest.Address = encryptionService.Decrypt(src.Address);
                    dest.Ward = encryptionService.Decrypt(src.Ward);
                    dest.District = encryptionService.Decrypt(src.District);
                    dest.City = encryptionService.Decrypt(src.City);
                });

            CreateMap<ApplicationUser, GetAllUserRes>();
        }
    }
}
