using AutoMapper;
using BookStore.Application.Interface;
using BookStore.Application.InterfacesRepository;
using BookStore.Domain.DTOs;
using BookStore.Domain.Models;
using BookStore.Domain.Queries;
using BookStore.Domain.Result;

using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Application.Services
{
    public class PaymentProfileService : IPaymentProfileService
    {
        private readonly IUnitOfWork _data;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        public PaymentProfileService(IUnitOfWork data, UserManager<ApplicationUser> userManager,IMapper mapper)
        {
            _data = data;
            _userManager = userManager;
            _mapper=mapper;
        }
        public async Task<Result<IEnumerable<GetPaymentProfileRes>>> GetAllPaymentProfileAsync(string userName)
        {
            Result<IEnumerable<GetPaymentProfileRes>> result = new Result<IEnumerable<GetPaymentProfileRes>>();
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                result.Success = false;
                result.Message = "Người dùng không tồn tại!";
                result.Data = null;
                return result;
            }
            QueryOptions<PaymentProfile> options = new QueryOptions<PaymentProfile>
            {
                Where=c=>c.UserId.Equals(user.Id),
                OrderBy=c=>c.CreatedTime,
                OrderByDirection="desc"
            };
           
            var data = await _data.PaymentProfile.ListAllAsync(options);
            result.Success = true;
            result.Data = _mapper.Map<IEnumerable<GetPaymentProfileRes>>(data);
            return result;
        }
        public async Task<Result<GetPaymentProfileRes>> GetPaymentProfileByNameAsync(string userName,string name)
        {
            Result<GetPaymentProfileRes> result = new Result<GetPaymentProfileRes>();
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                result.Success = false;
                result.Message = "Người dùng không tồn tại!";
                result.Data = null;
                return result;
            }
            QueryOptions<PaymentProfile> options = new QueryOptions<PaymentProfile>
            {
                Where = c => c.UserId.Equals(user.Id)&&c.Nickname.Equals(name)
            };
            var data = await _data.PaymentProfile.GetAsync(options);
            result.Success = true;
            result.Data = _mapper.Map<GetPaymentProfileRes>(data);
            return result;
        }
        public async Task<Result<PaymentProfile>> RemovePaymentProfileAsync(Guid id)
        {
            Result<PaymentProfile> result = new Result<PaymentProfile>();
            var paymentProfile = await _data.PaymentProfile.GetAsync(new QueryOptions<PaymentProfile>
            {
                Where = c => c.PaymentProfileId.Equals(id)
            });
            if (paymentProfile == null)
            {
                result.Success = false;
                result.Message = "Hồ sơ không tồn tại";
                result.Data = null;
                return result;
            }
            _data.PaymentProfile.Remove(paymentProfile);
            await _data.SaveAsync();
            result.Success = true;
            result.Message = "Xóa hồ sơ thành công";
            return result;
        }
    }
}
