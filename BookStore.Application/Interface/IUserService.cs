using BookStore.Domain.DTOs;
using BookStore.Domain.Models;
using BookStore.Domain.Queries;
using BookStore.Domain.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Application.Interface
{
    public interface IUserService
    {
        Task<Result<ApplicationUser>> Register(RegisterReq register);
        Task<Result<LoginRes>> Login(LoginReq login);
        Task<Result<ApplicationUser>> ConfirmMail(string token, string userName);
        Task<Result<ApplicationUser>> ChangePassword(string userName, ChangPasswordReq param);
        Task<Result<ApplicationUser>> UpdateInformation(string userName, UpdateInformationReq param);
        Task<Result<ApplicationUser>> ForgotPassword(ForgotPasswordReq param);
        Task<Result<ApplicationUser>> ResetPassword(ResetPasswordReq param);
        Task<Result<TwoFactorAuthenticationRes>> GetTwoFactorAuthenticationCode(string userName);
        Task<Result<TwoFactorAuthenticationRes>> TwoFactorAuthentication(string userName, EnableTwoFaReq passcode);
        Task<Result<LoginRes>> Verify2FA(string userName, TwoFAReq passcode);
        Task<Result<string>> VerifyOtp(string userName, string Otp);
        Task<Result<GetInformationRes>> GetInformation(string userName);
        Task<Result<ApplicationUser>> ChangeEmail(string userName);
        Task<ClaimsPrincipal> GetPrincipalFromExpiredToken(string accessToken);
        Task<Result<TwoFactorAuthenticationRes>> Disable2FA(string userName, EnableTwoFaReq passcode);
        Task<Result<LoginRes>> RefreshToken(string userName, RefreshTokenReq req);
        Task<Result<ApplicationUser>> ChangeEmail(ChangeEmailReq param, string userName);
        Task<Result<PaginationResponse<GetAllUserRes>>> GetAllUsersAsync(int page, int size, string? term);
        Task<Result<string>> UpdateUser(string id, UpdateUserReq req);
        Task<Result<GetAllUserRes>> GetUserInformationAsync(string id);

    }
}
