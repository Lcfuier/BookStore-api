using BookStore.Application.Interface;
using BookStore.Domain.Models;
using BookStore.Domain.Result;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using BookStore.Domain.DTOs;

namespace BookStore.Api.Controllers
{
    [Route("api/[controller]")]
    public class AuthenticateController : Controller
    {
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        public AuthenticateController(IUserService userService, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userService = userService;
            _userManager = userManager;
            _configuration = configuration;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterReq register)
        {
            var result = await _userService.Register(register);
            if (result.Success == false)
            {
                return BadRequest(result);
            }
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(result.Data);
            //code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Action(nameof(ConfirmEmail), "Authenticate", new { token = code, userName = result.Data.NormalizedUserName }, Request.Scheme);
            /*string url = $"{_configuration["UrlConfirmEmail:Url"]}?username={Uri.EscapeDataString(result.Data.NormalizedUserName)}&token={Uri.EscapeDataString(code)}";
            await SendEmailAsync(result.Data.Email, "Confirm your email",
            $"Please confirm your account by <a href='{url}'>clicking here</a>.");*/
            await SendEmailAsync(result.Data.Email, "Confirm your email",
            $"Please confirm your account by <a href='{callbackUrl}'>clicking here</a>.");
            return Ok(new Result<string>
            {
                Data = null,
                Message = result.Message,
                Success = result.Success,
            });
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginReq login)
        {
            var result = await _userService.Login(login);
            if (result.Success == false)
            {
                return Unauthorized(result);
            }
            else
            {
                if (result.Data is null)
                {
                    HttpContext.Session.SetString("UserName", login.UserName);
                    return Ok(result);
                }
                else
                {
                    return Ok(result);
                }

            }
        }
        [HttpGet("Confirm-Email")]
        public async Task<IActionResult> ConfirmEmail(string token, string userName)
        {
            var result = await _userService.ConfirmMail(token, userName);
            if (result.Success == false)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpPost("Forgot-Password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordReq param)
        {
            var result = await _userService.ForgotPassword(param);
            if (result.Success == false)
            {
                return BadRequest(result);
            }
            var token = await _userManager.GenerateUserTokenAsync(result.Data, TokenOptions.DefaultPhoneProvider, "ResetPassword");
            HttpContext.Session.SetString("UserNameForgotPassWord", param.UserName);
            await SendEmailAsync(result.Data.Email, "Reset your password",
           $"Your reset password OTP : {token}");
            return Ok(new Result<string>
            {
                Message = result.Message,
                Success = result.Success,
            });
        }
        [HttpPost("Verify-Otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] EnterOtpReq req)
        {
            var userName = HttpContext.Session.GetString("UserNameForgotPassWord");
            var result = await _userService.VerifyOtp(req.UserName, req.Otp);
            if (result.Success == false)
            {
                return BadRequest(result);
            }
            HttpContext.Session.SetString("OtpResetPassword", "True");
            return Ok(new Result<string>
            {
                Message = result.Message,
                Success = result.Success,
            });
        }
        [HttpPost("Reset-Password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordReq req)
        {
            var result = await _userService.ResetPassword(req);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            HttpContext.Session.SetString("OtpResetPassword", "");
            return Ok(result);
        }
        [HttpPost("Verify-2FA")]
        public async Task<IActionResult> Verify2FA([FromBody] TwoFAReq passcode)
        {
            var userName = passcode.UserName;
            var result = await _userService.Verify2FA(userName, passcode);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            else
            {
                return Ok(result);
            }
        }
        [HttpPost("refresh-token")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenReq req)
        {
            if (req.AccessToken is null)
            {
                return BadRequest(new Result<string>()
                {
                    Success = false,
                    Message = "access token is null",
                    Data = null
                });
            }
            var principal = await _userService.GetPrincipalFromExpiredToken(req.AccessToken);
            var userName = principal.FindFirstValue(ClaimTypes.Name);
            var result = await _userService.RefreshToken(userName, req);
            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }
        private async Task<bool> SendEmailAsync(string email, string subject, string confirmLink)
        {
            try
            {
                MailMessage message = new MailMessage();
                SmtpClient smtpClient = new SmtpClient();
                message.From = new MailAddress("phamkhanhduy.contact@gmail.com");
                message.To.Add(email);
                message.Subject = subject;
                message.IsBodyHtml = true;
                message.Body = confirmLink;

                smtpClient.Port = 587;
                smtpClient.Host = "smtp.gmail.com";

                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential("phamkhanhduy.contact@gmail.com", "vkqvitiruymstkqr");
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.Send(message);
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }


    }
}
