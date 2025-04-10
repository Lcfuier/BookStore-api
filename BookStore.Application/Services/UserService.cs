using BookStore.Domain.Models;
using BookStore.Infrastructure.Interface;
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
namespace BookStore.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _data;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        public UserService(IUnitOfWork data, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _data = data;
            _userManager = userManager;
            _configuration = configuration;
        }
        public async Task<Result<ApplicationUser>> Register(RegisterReq register)
        {
            Result<ApplicationUser> result = new Result<ApplicationUser>();
            if (string.IsNullOrEmpty(register.Password) || string.IsNullOrEmpty(register.EmailAddress) || string.IsNullOrEmpty(register.LastName) || string.IsNullOrEmpty(register.FirstName) || string.IsNullOrEmpty(register.PhoneNumber) || string.IsNullOrEmpty(register.UserName))
            {
                result.Success = false;
                result.Message = "Lost Infomation";
                result.Data = null;
                return result;
            }
            var userInDb = await _userManager.FindByEmailAsync(register.EmailAddress);
            if (userInDb != null)
            {
                result.Success = false;
                result.Message = "Email is already existing";
                result.Data = null;
                return result;
            }
            userInDb = await _userManager.FindByNameAsync(register.UserName);
            if (userInDb != null)
            {
                result.Success = false;
                result.Message = "UserName is already existing";
                result.Data = null;
                return result;
            }
            var user = new ApplicationUser { UserName = register.UserName, Email = register.EmailAddress,FirstName=register.FirstName,LastName=register.LastName,PhoneNumber=register.PhoneNumber,CreatedTime= DateTime.UtcNow };
            try
            {
                var req = await _userManager.CreateAsync(user, register.Password);
                if (req.Succeeded)
                {
                    result.Success = true;
                    result.Message = "Create User SuccessFul";
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
            Result<LoginRes> result = new Result<LoginRes>();
            if (string.IsNullOrEmpty(login.Password) || string.IsNullOrEmpty(login.UserName))
            {
                result.Success = false;
                result.Message = "Required username and password";
                result.Data = null;
                return result;
            }
            var user = await _userManager.FindByNameAsync(login.UserName);
            if (user is null)
            {
                result.Success = false;
                result.Message = "Username not exist";
                result.Data = null;
            }
            else if (user != null && await _userManager.CheckPasswordAsync(user, login.Password))
            {
                if (!user.EmailConfirmed)
                {
                    result.Success = false;
                    result.Message = "Email has not been confirmed";
                    result.Data = null;
                    return result;
                }
                else
                {
                    if (user.TwoFactorGoogleEnabled == true)
                    {
                        /*TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
                        string UserUniqueKey = user.TwoFactorGoogleCode;
                        bool isValid = tfa.ValidateTwoFactorPIN(UserUniqueKey, login.passcode) ;
                        if (!isValid)
                        {
                            result.Success = false;
                            result.Message = "Token is invalid";
                            result.Data = null;
                            return result;  
                        }*/
                        result.Data = null;
                        result.Message = "Enter 2FA code";
                    }
                    else
                    {
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
                        var jwtToken = GetToken(authClaim);

                        result.Data = new LoginRes()
                        {
                            AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                            RefreshToken = GenerateRefreshToken(),
                        };
                        result.Message = "Login successful";
                        user.RefreshToken = result.Data.RefreshToken;
                        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);
                        await _userManager.UpdateAsync(user);
                    }

                    result.Success = true;
                }

            }
            else
            {
                result.Success = false;
                result.Message = "Password is incorrect";
                result.Data = null;
            }
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
                    result.Message = "Confirm Email Sucessful";
                    result.Data = null;
                }
            }
            else
            {
                result.Success = false;
                result.Message = "User name not exist";
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
                result.Message = "UserName not exist";
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
                        result.Message = "Update password successful";
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
                    result.Message = "Current password is incorrect";
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
                result.Message = "userName not exist";
                result.Data = null;
            }
            else
            {
                user.Email = param.EmailAddress;
                user.PhoneNumber = param.PhoneNumber;
                user.LastName= param.LastName;
                user.FirstName= param.FirstName;
                user.ModifiedTime = DateTime.UtcNow;
                user.ModifiedBy = user.UserName;
                var req = await _userManager.UpdateAsync(user);
                if (req.Succeeded)
                {
                    result.Success = true;
                    result.Message = "Update information successful";
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
                result.Message = "Required userName";
                result.Data = null;
                return result;
            }
            var user = await _userManager.FindByNameAsync(param.UserName);
            if (user is null)
            {
                result.Success = false;
                result.Message = "user not exist";
                result.Data = null;
            }
            else
            {
                result.Success = true;
                result.Message = "Please check your email to reset your password";
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
                result.Message = "Unknow user";
                result.Data = null;
                return result;
            }
            var user = await _userManager.FindByNameAsync(userName);
            if (user is null)
            {
                result.Success = false;
                result.Message = "userName not exist";
                result.Data = null;
            }

            var isValidOtp = await _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultPhoneProvider, "ResetPassword", Otp);
            if (!isValidOtp)
            {
                result.Success = false;
                result.Message = "Invalid Code";
                result.Data = null;
            }
            else
            {
                result.Success = true;
                result.Message = "successful, change your password";
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
                result.Message = "Unknow user";

                return result;
            }

            var user = await _userManager.FindByNameAsync(param.UserName);
            if (user is null)
            {
                result.Success = false;
                result.Message = "User not exist";

            }
            else
            {
                if (!await _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultPhoneProvider, "ResetPassword", param.Otp))
                {
                    result.Success = false;
                    result.Message = "Please verify OTP again";

                    return result;
                }
                try
                {
                    await _userManager.RemovePasswordAsync(user);
                    var req = await _userManager.AddPasswordAsync(user, param.Password);
                    if (req.Succeeded)
                    {
                        result.Success = true;
                        result.Message = "Reset password successful";
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
                result.Message = "User not exist";
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
                result.Message = "successful";
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
                    result.Message = "successful";
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
                result.Message = "invalid code";
                result.Success = false;
            }
            // Step 4: Return the QR code and manual key as a response
            return result;
        }
        public async Task<Result<LoginRes>> Verify2FA(string userName, TwoFAReq passcode)
        {
            Result<LoginRes> result = new Result<LoginRes>();
            if (string.IsNullOrEmpty(userName))
            {
                result.Success = false;
                result.Message = "Unknow user";
                result.Data = null;
                return result;
            }

            var user = new ApplicationUser();
            user = await _userManager.FindByNameAsync(userName);
            if (user is null)
            {
                result.Success = false;
                result.Message = "user not exist";
                result.Data = null;
            }

            if (user.TwoFactorGoogleEnabled == true)
            {
                TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
                string UserUniqueKey = user.TwoFactorGoogleCode;
                bool isValid = tfa.ValidateTwoFactorPIN(UserUniqueKey, passcode.Passcode);
                if (!isValid)
                {
                    result.Success = false;
                    result.Message = "Token is invalid";
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
                    foreach (var role in userRoles)
                    {
                        authClaim.Add(new Claim(ClaimTypes.Role, role));
                    }
                    var jwtToken = GetToken(authClaim);
                    result.Success = true;
                    result.Message = "Login successful";
                    result.Data = new LoginRes()
                    {
                        AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                        RefreshToken = GenerateRefreshToken(),
                    };
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
                    user.TwoFactorGoogleEnabled = false;
                    _data.User.Update(user);
                    await _data.SaveAsync();
                    result.Message = "successful";
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
                result.Message = "invalid code";
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
                result.Message = "Unknow user";
                result.Data = null;
                return result;
            }
            var user = await _userManager.FindByNameAsync(userName);
            if (user is null)
            {
                result.Success = false;
                result.Message = "User not exist";
                result.Data = null;
                return result;
            }
            result.Message = "Successful";
            result.Success = true;
            result.Data = new GetInformationRes
            {
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                TwoFactorGoogleEnabled = user.TwoFactorGoogleEnabled,
                FirstName=user.FirstName,
                LastName=user.LastName,
            };
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
                ValidateLifetime = false // Ignore expiration
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
                result.Message = "Invalid token";
                result.Data = null;
                return result;
            }
            var token = handler.ReadJwtToken(req.AccessToken);
            if (token.ValidTo > DateTime.UtcNow)
            {
                result.Success = true;
                result.Message = "token is not expired";
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
                result.Message = "User not exist";
                result.Data = null;
            }

            if (user == null || user.RefreshToken != req.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                result.Success = false;
                result.Message = "Invalid refresh token";
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
    }
}
