using BookStore.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BookStore.Domain.Result;
using BookStore.Domain.DTOs;
using Microsoft.Win32;
using System.IdentityModel.Tokens.Jwt;
using Google.Authenticator;
using Microsoft.IdentityModel.Tokens;
using BookStore.Application.Interface;
using BookStore.Domain.Constants;
using BookStore.Domain.Queries;
using Google.Apis.Drive.v3.Data;
using Microsoft.Extensions.Options;
using AutoMapper;
using System.Text.RegularExpressions;
using BookStore.Application.InterfacesRepository;
namespace BookStore.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _data;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IEncryptionService _encryptionService;
        public UserService(IUnitOfWork data, UserManager<ApplicationUser> userManager, IConfiguration configuration, IMapper mapper, IEncryptionService encryptionService)
        {
            _data = data;
            _userManager = userManager;
            _configuration = configuration;
            _mapper = mapper;
            _encryptionService = encryptionService;
        }
        public async Task<Result<ApplicationUser>> Register(RegisterReq register)
        {
            Result<ApplicationUser> result = new Result<ApplicationUser>();
            if (string.IsNullOrWhiteSpace(register.Password) || IsUsernameValid(register.Password))
            {
                result.Success = false;
                result.Message = "Tên đăng nhập hoặc mật khẩu không lệ!";
                result.Data = null;
                return result;
            }
            register.UserName = register.UserName.Trim();
            register.Password = register.Password.Trim();
            if (string.IsNullOrEmpty(register.Password) || string.IsNullOrEmpty(register.EmailAddress) || string.IsNullOrEmpty(register.LastName) || string.IsNullOrEmpty(register.FirstName) || string.IsNullOrEmpty(register.PhoneNumber) || string.IsNullOrEmpty(register.UserName))
            {
                result.Success = false;
                result.Message = "Hãy nhập đủ thông tin!";
                result.Data = null;
                return result;
            }
            var userInDb = await _userManager.FindByEmailAsync(register.EmailAddress);
            if (userInDb != null)
            {
                result.Success = false;
                result.Message = "Email đã tồn tại !";
                result.Data = null;
                return result;
            }
            userInDb = await _userManager.FindByNameAsync(register.UserName);
            if (userInDb != null)
            {
                result.Success = false;
                result.Message = "UserName đã tồn tại !";
                result.Data = null;
                return result;
            }
            var user = new ApplicationUser { UserName = register.UserName, Email = register.EmailAddress,FirstName=register.FirstName,LastName=register.LastName,PhoneNumber=register.PhoneNumber,CreatedTime= DateTime.UtcNow };
            try
            {
                var req = await _userManager.CreateAsync(user, register.Password);
                if (req.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, Roles.User);
                    result.Success = true;
                    result.Message = "Tạo tài khoản thành công!";
                    result.Data = user;
                }
                else
                {
                    result.Success = false;
                    result.Message = req.ToString();
                    result.Data = null;
                }

            }
            catch (Exception ex)
            {
                result.Message = ex.Message.ToString();
                result.Success = false;
            }
            return result;
        }
        public async Task<Result<LoginRes>> Login(LoginReq login)
        {
            var result = new Result<LoginRes>();

            if (string.IsNullOrWhiteSpace(login.Password) || IsUsernameValid(login.Password))
            {
                result.Success = false;
                result.Message = "Tên đăng nhập hoặc mật khẩu không hợp lệ!";
                return result;
            }

            login.UserName = login.UserName.Trim();
            login.Password = login.Password.Trim();

            if (string.IsNullOrEmpty(login.UserName) || string.IsNullOrEmpty(login.Password))
            {
                result.Success = false;
                result.Message = "Hãy nhập đầy đủ tên người dùng và mật khẩu.";
                return result;
            }

            var user = await _userManager.FindByNameAsync(login.UserName);
            if (user == null)
            {
                result.Success = false;
                result.Message = "Người dùng không tồn tại!";
                return result;
            }

            if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
            {
                var remaining = user.LockoutEnd.Value - DateTimeOffset.UtcNow;
                result.Success = false;
                result.Message = $"Tài khoản bị khóa. Hết hạn sau {remaining.TotalMinutes:N0} phút.";
                return result;
            }

            if (!await _userManager.CheckPasswordAsync(user, login.Password))
            {
                result.Success = false;
                result.Message = "Mật khẩu không chính xác!";
                return result;
            }

            if (!user.EmailConfirmed)
            {
                result.Success = false;
                result.Message = "Email chưa được xác minh!";
                return result;
            }

            if (user.TwoFactorGoogleEnabled == true && login.Passcode==null)
            {
                result.Success = true;
                result.Message = "Yêu cầu mã xác thực 2 bước.";
                result.Data = null;
                // FE sẽ hiểu để hiện form nhập OTP
                return result;
            }

            if(user.TwoFactorGoogleEnabled == true && login.Passcode != null)
            {
                TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
                string UserUniqueKey = user.TwoFactorGoogleCode;
                bool isValid = tfa.ValidateTwoFactorPIN(UserUniqueKey, login.Passcode) ;
                if (!isValid)
                {
                    result.Success = false;
                    result.Message = "Mã xác thực không hợp lệ!";
                    result.Data = null;
                    return result;
                }
            }
            // Nếu không bật 2FA → cấp token ngay
            var authClaims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.NormalizedUserName),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var jwtToken = GetToken(authClaims);

            result.Data = new LoginRes
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                RefreshToken = GenerateRefreshToken()
            };
            result.Success = true;
            result.Message = "Đăng nhập thành công!";

            user.RefreshToken = result.Data.RefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);
            await _userManager.UpdateAsync(user);

            return result;
        }

        public async Task<Result<ApplicationUser>> ConfirmMail(string token, string userName)
        {
            Result<ApplicationUser> result = new Result<ApplicationUser>();
            var user = await _userManager.FindByNameAsync(userName);
            if (user != null)
            {
                var req = await _userManager.ConfirmEmailAsync(user, token);
                if (req.Succeeded)
                {
                    result.Success = true;
                    result.Message = "Xác nhận email thành công!";
                    result.Data = null;
                }
                else
                {
                    result.Success = false;
                    result.Message = req.Errors.ToString();
                    result.Data = null;
                }
            }
            else
            {
                result.Success = false;
                result.Message = "Tên người dùng không tồn tại!";
                result.Data = null;
            }
            return result;
        }
        public async Task<Result<ApplicationUser>> ChangePassword(string userName, ChangPasswordReq param)
        {
            Result<ApplicationUser> result = new Result<ApplicationUser>();
            var user = await _userManager.FindByNameAsync(userName);
            if (user is null)
            {
                result.Success = false;
                result.Message = "Tên người dùng không tồn tại!";
                result.Data = null;
            }
            else
            {
                if (await _userManager.CheckPasswordAsync(user, param.CurrentPassword))
                {
                    var req = await _userManager.ChangePasswordAsync(user, param.CurrentPassword, param.NewPassword);
                    if (req.Succeeded)
                    {
                        result.Success = true;
                        result.Message = "Cập nhật mật khẩu thành công!";
                        result.Data = null;
                        user.ModifiedTime= DateTime.UtcNow;
                        await _userManager.UpdateAsync(user);
                    }
                    else
                    {
                        result.Success = false;
                        result.Message = req.Errors.ToString();
                        result.Data = null;
                    }
                }
                else
                {
                    result.Success = false;
                    result.Message = "Mật khẩu hiện tại không đúng !";
                    result.Data = null;
                }
            }
            return result;
        }
        public async Task<Result<ApplicationUser>> UpdateInformation(string userName, UpdateInformationReq param)
        {
            Result<ApplicationUser> result = new Result<ApplicationUser>();
            var user = await _userManager.FindByNameAsync(userName);
            if (user is null)
            {
                result.Success = false;
                result.Message = "Không tìm thấy người dùng!";
                result.Data = null;
            }
            else
            {
                user.PhoneNumber = param.PhoneNumber;
                user.LastName= param.LastName;
                user.FirstName= param.FirstName;
                user.ModifiedTime = DateTime.UtcNow;
                user.ModifiedBy = user.UserName;
                var req = await _userManager.UpdateAsync(user);
                if (req.Succeeded)
                {
                    result.Success = true;
                    result.Message = "Cập nhật thông tin thành công !";
                    result.Data = null;
                }
                else
                {
                    result.Success = false;
                    result.Message = req.Errors.ToString();
                    result.Data = null;
                }
            }
            return result;
        }
        public async Task<Result<ApplicationUser>> ForgotPassword(ForgotPasswordReq param)
        {
            Result<ApplicationUser> result = new Result<ApplicationUser>();
            if (string.IsNullOrEmpty(param.UserName))
            {
                result.Success = false;
                result.Message = "Hãy nhập tên người dùng!";
                result.Data = null;
                return result;
            }
            var user = await _userManager.FindByNameAsync(param.UserName);
            if (user is null)
            {
                result.Success = false;
                result.Message = "Tên người dùng không tồn tại!"; ;
                result.Data = null;
            }
            else
            {
                result.Success = true;
                result.Message = "Kiểm tra email của bạn để đặt lại mật khẩu!";
                result.Data = user;
            }
            return result;
        }
        public async Task<Result<string>> VerifyOtp(string userName, string Otp)
        {
            Result<string> result = new Result<string>();
            if (string.IsNullOrEmpty(userName))
            {
                result.Success = false;
                result.Message = "Tên người dùng không hợp lệ!";
                result.Data = null;
                return result;
            }
            var user = await _userManager.FindByNameAsync(userName);
            if (user is null)
            {
                result.Success = false;
                result.Message = "không tìm thấy người dùng!";
                result.Data = null;
            }

            var isValidOtp = await _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultPhoneProvider, "veriyOtp", Otp);
            if (!isValidOtp)
            {
                result.Success = false;
                result.Message = "Mã xác nhật không hợp lệ!";
                result.Data = null;
            }
            else
            {
                result.Success = true;
                result.Data = Otp;
            }
            return result;
        }
        public async Task<Result<ApplicationUser>> ResetPassword(ResetPasswordReq param)
        {
            Result<ApplicationUser> result = new Result<ApplicationUser>();
            result.Data = null;
            if (string.IsNullOrEmpty(param.UserName))
            {
                result.Success = false;
                result.Message = "Tên người dùng không tồn tại!";

                return result;
            }

            var user = await _userManager.FindByNameAsync(param.UserName);
            if (user is null)
            {
                result.Success = false;
                result.Message = "Tên người dùng không tồn tại!";

            }
            else
            {
                if (!await _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultPhoneProvider, "veriyOtp", param.Otp))
                {
                    result.Success = false;
                    result.Message = "Hãy nhập lại OTP!";

                    return result;
                }
                try
                {
                    await _userManager.RemovePasswordAsync(user);
                    var req = await _userManager.AddPasswordAsync(user, param.Password);
                    if (req.Succeeded)
                    {
                        result.Success = true;
                        result.Message = "Đặt lại mật khẩu thành công!";
                    }
                    else
                    {
                        result.Success = false;
                        result.Message = req.ToString();
                    }


                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Message = ex.Message.ToString();
                }
            }
            return result;
        }
        public async Task<Result<TwoFactorAuthenticationRes>> GetTwoFactorAuthenticationCode(string userName)
        {
            Result<TwoFactorAuthenticationRes> result = new Result<TwoFactorAuthenticationRes>();
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                result.Success = false;
                result.Message = "Người dùng không tồn tại!";
                result.Data = null;
            }
            string UserUniqueKey = Guid.NewGuid().ToString();
            TwoFactorAuthenticator TwoFacAuth = new TwoFactorAuthenticator();
            var setupInfo = TwoFacAuth.GenerateSetupCode($"{userName.ToString()}", userName, UserUniqueKey, false, 300);
            // Step 2: Generate a QR code URI
            try
            {
                user.TwoFactorGoogleCode = UserUniqueKey;
                _data.User.Update(user);
                await _data.SaveAsync();
                result.Success = true;
                TwoFactorAuthenticationRes data = new TwoFactorAuthenticationRes();
                data.ManualKey = setupInfo.ManualEntryKey;
                data.QrCodeImageBase64 = setupInfo.QrCodeSetupImageUrl;
                result.Data = data;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message.ToString();
            }
            // Step 4: Return the QR code and manual key as a response
            return result;
        }
        public async Task<Result<TwoFactorAuthenticationRes>> TwoFactorAuthentication(string userName, EnableTwoFaReq passcode)
        {
            Result<TwoFactorAuthenticationRes> result = new Result<TwoFactorAuthenticationRes>();
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                result.Success = false;
                result.Message = "User not exist";
                result.Data = null;
            }
            TwoFactorAuthenticator TwoFacAuth = new TwoFactorAuthenticator();
            string UserUniqueKey = user.TwoFactorGoogleCode;
            bool isValid = TwoFacAuth.ValidateTwoFactorPIN(UserUniqueKey, passcode.Passcode);
            if (isValid)
            {
                try
                {
                    user.TwoFactorGoogleEnabled = true;
                    _data.User.Update(user);
                    await _data.SaveAsync();
                    result.Message = "Kích hoạt xác thực 2 bước thành công!";
                    result.Success = true;
                }
                catch (Exception ex)
                {
                    result.Message = ex.Message.ToString();
                    result.Success = false;
                }
            }
            else
            {
                result.Message = "Mã xác thực không hợp kệ!";
                result.Success = false;
            }
            // Step 4: Return the QR code and manual key as a response
            return result;
        }
        public async Task<Result<LoginRes>> Verify2FA(LoginReq login)
        {
            Result<LoginRes> result = new Result<LoginRes>();

            if (string.IsNullOrWhiteSpace(login.Password) || IsUsernameValid(login.Password))
            {
                result.Success = false;
                result.Message = "Tên đăng nhập hoặc mật khẩu không hợp lệ!";
                return result;
            }

            login.UserName = login.UserName.Trim();
            login.Password = login.Password.Trim();

            if (string.IsNullOrEmpty(login.UserName) || string.IsNullOrEmpty(login.Password))
            {
                result.Success = false;
                result.Message = "Hãy nhập đầy đủ tên người dùng và mật khẩu.";
                return result;
            }

            var user = await _userManager.FindByNameAsync(login.UserName);
            if (user == null)
            {
                result.Success = false;
                result.Message = "Người dùng không tồn tại!";
                return result;
            }

            if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
            {
                var remaining = user.LockoutEnd.Value - DateTimeOffset.UtcNow;
                result.Success = false;
                result.Message = $"Tài khoản bị khóa. Hết hạn sau {remaining.TotalMinutes:N0} phút.";
                return result;
            }

            if (!await _userManager.CheckPasswordAsync(user, login.Password))
            {
                result.Success = false;
                result.Message = "Mật khẩu không chính xác!";
                return result;
            }

            if (!user.EmailConfirmed)
            {
                result.Success = false;
                result.Message = "Email chưa được xác minh!";
                return result;
            }
            if (user.TwoFactorGoogleEnabled == true)
            {
                TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
                string UserUniqueKey = user.TwoFactorGoogleCode;
                bool isValid = tfa.ValidateTwoFactorPIN(UserUniqueKey, login.Passcode);
                if (!isValid)
                {
                    result.Success = false;
                    result.Message = "Mã xác thực không hợp lệ!";
                    result.Data = null;
                    return result;
                }
                else
                {
                    var authClaim = new List<Claim>
{
    new Claim(ClaimTypes.Name,user.NormalizedUserName),
    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
};
                    var userRoles = await _userManager.GetRolesAsync(user);
                    if (!userRoles.Any())
                    {
                        // Debug log hoặc breakpoint
                        Console.WriteLine("User has no roles assigned.");
                    }
                    foreach (var role in userRoles)
                    {
                        authClaim.Add(new Claim(ClaimTypes.Role, role));
                    }
                    var jwtToken = GetToken(authClaim);

                    result.Data = new LoginRes()
                    {
                        AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                        RefreshToken = GenerateRefreshToken(),
                    };
                    result.Success = true;
                    user.RefreshToken = result.Data.RefreshToken;
                    user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);
                    await _userManager.UpdateAsync(user);
                    return result;
                }
            }
            return result;
        }
        public async Task<Result<TwoFactorAuthenticationRes>> Disable2FA(string userName, EnableTwoFaReq passcode)
        {
            Result<TwoFactorAuthenticationRes> result = new Result<TwoFactorAuthenticationRes>();
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                result.Success = false;
                result.Message = "Người dùng không tồn tại!";
                result.Data = null;
            }
            TwoFactorAuthenticator TwoFacAuth = new TwoFactorAuthenticator();
            string UserUniqueKey = user.TwoFactorGoogleCode;
            bool isValid = TwoFacAuth.ValidateTwoFactorPIN(UserUniqueKey, passcode.Passcode);
            if (isValid)
            {
                try
                {
                    user.TwoFactorGoogleEnabled = false;
                    _data.User.Update(user);
                    await _data.SaveAsync();
                    result.Message = "Hủy kích hoạt xác thực 2 lớp thành công!";
                    result.Success = true;
                }
                catch (Exception ex)
                {
                    result.Message = ex.Message.ToString();
                    result.Success = false;
                }
            }
            else
            {
                result.Message = "Mã xác thực không hợp lệ !";
                result.Success = false;
            }
            // Step 4: Return the QR code and manual key as a response
            return result;
        }
        public async Task<Result<GetInformationRes>> GetInformation(string userName)
        {
            Result<GetInformationRes> result = new Result<GetInformationRes>();
            if (string.IsNullOrEmpty(userName))
            {
                result.Success = false;
                result.Message = "Hãy nhập tên người dùng!";
                result.Data = null;
                return result;
            }
            var user = await _userManager.FindByNameAsync(userName);
            if (user is null)
            {
                result.Success = false;
                result.Message = "Người dùng không tồn tại!";
                result.Data = null;
                return result;
            }
            result.Success = true;
            result.Data = new GetInformationRes
            {
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                TwoFactorGoogleEnabled = user.TwoFactorGoogleEnabled,
                FirstName=user.FirstName,
                LastName=user.LastName,
                userName=user.UserName
            };
            return result;
        }
        public async Task<Result<ApplicationUser>> ChangeEmail(ChangeEmailReq param,string userName)
        {
            Result<ApplicationUser> result = new Result<ApplicationUser>();
            result.Data = null;
            if (string.IsNullOrEmpty(userName))
            {
                result.Success = false;
                result.Message = "Tên người dùng không hợp lệ !";

                return result;
            }

            var user = await _userManager.FindByNameAsync(userName);
            if (user is null)
            {
                result.Success = false;
                result.Message = "Người dùng không tồn tại !";

            }
            var checkExistUser = await _userManager.FindByEmailAsync(param.Email);
            if (checkExistUser is not null)
            {
                result.Success = false;
                result.Message = "Email mới đã được sử dụng !";

            }
            else
            {
                if (!await _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultPhoneProvider, "veriyOtp", param.Otp))
                {
                    result.Success = false;
                    result.Message = "Hãy xác nhận lại Otp";

                    return result;
                }
                try
                {
                    user.Email = param.Email;
                    user.NormalizedEmail = param.Email;
                    var req = await _userManager.UpdateAsync(user);
                    if (req.Succeeded)
                    {
                        result.Success = true;
                        result.Message = "Đổi email thành công";
                    }
                    else
                    {
                        result.Success = false;
                        result.Message = req.ToString();
                    }


                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Message = ex.Message.ToString();
                }
            }
            return result;
        }
        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                expires: DateTime.Now.AddHours(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );
            return token;
        }
        // Helper method to generate a QR Code image as a byte array
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
        public async Task<ClaimsPrincipal> GetPrincipalFromExpiredToken(string accessToken)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = key,
                ValidateLifetime = false, // Ignore expiration
                RoleClaimType = ClaimTypes.Role
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
        public async Task<Result<LoginRes>> RefreshToken(string userName, RefreshTokenReq req)
        {
            var handler = new JwtSecurityTokenHandler();

            var result = new Result<LoginRes>();
            if (!handler.CanReadToken(req.AccessToken))
            {
                result.Success = false;
                result.Message = "Token không hợp lệ!";
                result.Data = null;
                return result;
            }
            var token = handler.ReadJwtToken(req.AccessToken);
            if (token.ValidTo > DateTime.UtcNow)
            {
                result.Success = true;
                result.Message = "Token chưa hết hạn";
                result.Data = new LoginRes()
                {
                    AccessToken = req.AccessToken,
                    RefreshToken = req.RefreshToken,
                };
                return result;
            }
            var user = await _userManager.FindByNameAsync(userName);
            if (user is null)
            {
                result.Success = false;
                result.Message = "Tên người dùng không tồn tại!";
                result.Data = null;
            }

            if (user == null || user.RefreshToken != req.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                result.Success = false;
                result.Message = "Refresh token không hợp lệ!";
                result.Data = null;
                return result;
            }
            var authClaim = new List<Claim>
            {
                new Claim(ClaimTypes.Name,user.NormalizedUserName),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
            };
            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                authClaim.Add(new Claim(ClaimTypes.Role, role));
            }
            result.Data = new LoginRes()
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(GetToken(authClaim)),
                RefreshToken = GenerateRefreshToken(),
            };

            user.RefreshToken = result.Data.RefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);
            result.Success = true;
            return result;
        }
        public async Task<Result<ApplicationUser>> ChangeEmail(string userName)
        {
            Result<ApplicationUser> result = new Result<ApplicationUser>();
            if (string.IsNullOrEmpty(userName))
            {
                result.Success = false;
                result.Message = "Tên người dùng không hợp lệ!";
                result.Data = null;
                return result;
            }
            var user = await _userManager.FindByNameAsync(userName);
            if (user is null)
            {
                result.Success = false;
                result.Message = "Người dùng không tồn tại!";
                result.Data = null;
            }
            else
            {
                result.Success = true;
                result.Message = "Kiếm tra email để đổi email mới !";
                result.Data = user;
            }
            return result;
        }
        public async Task<Result<PaginationResponse<GetAllUserRes>>> GetAllUsersAsync(int page, int size, string? term,string userName)
        {
            var userAdmin = await _userManager.FindByNameAsync(userName);
            if (userAdmin == null)
            {
                return new Result<PaginationResponse<GetAllUserRes>>
                {
                    Success = false,
                    Data = null,
                    Message = "Người dùng không tồn tại !"
                };
            }
            var rolesExist = await _userManager.GetRolesAsync(userAdmin);
            if (rolesExist.FirstOrDefault()?.ToLower() != Roles.Admin.ToLower())
            {
                return new Result<PaginationResponse<GetAllUserRes>>
                {
                    Success = false,
                    Data = null,
                    Message = "Không thể truy cập!"
                };
            }
            IEnumerable<ApplicationUser> data;
            QueryOptions<ApplicationUser> options = new QueryOptions<ApplicationUser>
            {
                Where= c=>c.UserName != "admin"
            };
            if (term != null)
            {
                options.Where = mi => mi.UserName.ToLower().Contains(term.ToLower()) ;
            }
            if (page < 1)
            {
                data = await _data.User.ListAllAsync(options);
            }

            else
            {
                options.PageNumber = page;
                options.PageSize = size;
                data = await _data.User.ListAllAsync(options);
            }
            var result = _mapper.Map<IEnumerable<GetAllUserRes>>(data);
            foreach (var item in result)
            {
                var user = await _userManager.FindByIdAsync(item.Id);
                var roles = await _userManager.GetRolesAsync(user);
                item.Role = roles.FirstOrDefault();
                item.LockoutEnd = user.LockoutEnd?.UtcDateTime ?? null;
            }
            PaginationResponse<GetAllUserRes> paginationResponse = new PaginationResponse<GetAllUserRes>
            {
                PageNumber = page,
                PageSize = size,
                // must be above the TotalRecords bc it has multiple Where clauses
                Items = result,
                TotalRecords = await _data.User.CountAsync()
            };
            return new Result<PaginationResponse<GetAllUserRes>>
            {
                Data = paginationResponse,
                Success = true
            };
        }
        public async Task<Result<string>> UpdateUser(string id,UpdateUserReq req,string userName)
        {
            var userAdmin = await _userManager.FindByNameAsync(userName);
            if (userAdmin == null)
            {
                return new Result<string>
                {
                    Success = false,
                    Data = null,
                    Message = "Người dùng không tồn tại !"
                };
            }
            var rolesExist = await _userManager.GetRolesAsync(userAdmin);
            if (rolesExist.FirstOrDefault()?.ToLower() != Roles.Admin.ToLower())
            {
                return new Result<string>
                {
                    Success = false,
                    Data = null,
                    Message = "Không thể truy cập!"
                };
            }
            Result<string> result = new Result<string>();
            if (string.IsNullOrEmpty(id))
            {
                result.Success = false;
                result.Message = "Tên người dùng không hợp lệ!";
                result.Data = null;
                return result;
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                result.Success = false;
                result.Message = "Người dùng không tồn tại!";
                result.Data = null;
            }
            if(req.isLockout==true)
            {
                DateTimeOffset currentDate = DateTimeOffset.UtcNow;
                DateTimeOffset lockoutEndDate = user.LockoutEnd ?? DateTimeOffset.MinValue;

                if (lockoutEndDate > currentDate)
                {
                    // unlock
                    user.LockoutEnd = DateTimeOffset.MinValue; // hoặc currentDate.AddYears(-100)
                    _data.User.Update(user);
                    await _data.SaveAsync();
                    result.Message = "Đã mở khóa cho tài khoản!";
                    result.Success = true;
                    return result;
                }
                else
                {
                    // lock
                    user.LockoutEnd = currentDate.AddYears(100); // vẫn là UTC
                    _data.User.Update(user);
                    await _data.SaveAsync();
                    result.Message = "Đã khóa tài khoản!";
                    result.Success = true;
                    return result;
                }

            }
            else
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    result.Message = removeResult.Errors.ToString();
                    result.Success = false;
                    return result;
                }
                var addResult = await _userManager.AddToRoleAsync(user, req.role);
                if (!addResult.Succeeded)
                {
                    result.Message = addResult.Errors.ToString();
                    result.Success = false;
                    return result;
                }
                result.Message = "Cập nhật quyền cho người dùng thành công!";
                result.Success = true;
                return result;
            }
            
        }
        public async Task<Result<GetAllUserRes>> GetUserInformationAsync(string id,string userName)
        {
          
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return new Result<GetAllUserRes>
                {
                    Success = false,
                    Data = null,
                    Message = "Người dùng không tồn tại !"
                };
            }
            var rolesExist = await _userManager.GetRolesAsync(user);
            if (rolesExist.FirstOrDefault()?.ToLower() != Roles.Admin.ToLower() )
            {
                return new Result<GetAllUserRes>
                {
                    Success = false,
                    Data = null,
                    Message = "Không thể truy cập!"
                };
            }
            ApplicationUser data;
            QueryOptions<ApplicationUser> options = new QueryOptions<ApplicationUser>
            {
                Where=c=>c.Id.Equals(id)
            };
            data = await _data.User.GetAsync(options);
           if(data == null)
            {
                return new Result<GetAllUserRes>
                {
                    Data = null,
                    Success = true,
                    Message="Không tìm thấy người dùng"
                };
            }
            var result = _mapper.Map<GetAllUserRes>(data);
            var roles = await _userManager.GetRolesAsync(data);
            result.Role = roles.FirstOrDefault();
            result.LockoutEnd = data.LockoutEnd?.UtcDateTime ?? null;
            
            return new Result<GetAllUserRes>
            {
                Data = result,
                Success = true
            };
        }
        public static bool IsUsernameValid(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;
            if (!IsValidUsername(username)) return false;
            if (ContainsEmoji(username)) return false;

            return true;
        }
        public static bool ContainsEmoji(string input)
        {
            var emojiRegex = new Regex(@"[\uD800-\uDBFF][\uDC00-\uDFFF]|[\u2100-\u27BF]|[\uFE00-\uFE0F]|\u3030|\u00AE|\u00A9|\u203C|\u2049|\u2122|\u2139|\u2194-\u21AA|\u231A-\u231B|\u23E9-\u23FA|\u24C2|\u25AA-\u25AB|\u25B6|\u25C0|\u25FB-\u25FE|\u2600-\u27BF|\u2934-\u2935|\u2B05-\u2B55|\u303D|\u3297|\u3299|\uD83C[\uDC00-\uDFFF]|\uD83D[\uDC00-\uDFFF]|\uD83E[\uDD00-\uDFFF]",
                RegexOptions.Compiled);
            return emojiRegex.IsMatch(input);
        }

        public static bool IsValidUsername(string username)
        {
            // Tên người dùng chỉ chứa chữ cái, số, dấu gạch dưới, dài 3-20 ký tự
            var regex = new Regex(@"^[a-zA-Z0-9_]{3,20}$");
            return regex.IsMatch(username);
        }

    }
}
