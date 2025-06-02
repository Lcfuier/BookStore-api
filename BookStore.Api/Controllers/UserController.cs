using BookStore.Application.Interface;
using BookStore.Application.Services;
using BookStore.Domain.DTOs;
using BookStore.Domain.Models;
using BookStore.Domain.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;

namespace BookStore.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : Controller
    {

        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        public UserController(IUserService userService,UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userService = userService;
            _userManager = userManager;
            _configuration = configuration;
        }
        [HttpPut("Change-Password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangPasswordReq param)
        {
            ClaimsIdentity? claimsIdentity = User.Identity as ClaimsIdentity;
            string userName = claimsIdentity.Name.ToString();
            var result = await _userService.ChangePassword(userName, param);
            if (result.Success)
            {
                return Ok(result);
            }
            else
                return BadRequest(result);
        }
        [HttpPut("Update-Information")]
        public async Task<IActionResult> UpdateInformation([FromBody] UpdateInformationReq param)
        {
            ClaimsIdentity? claimsIdentity = User.Identity as ClaimsIdentity;
            string userName = claimsIdentity.Name.ToString();
            var result = await _userService.UpdateInformation(userName, param);
            if (result.Success)
            {
                return Ok(result);
            }
            else
                return BadRequest(result);
        }
        [HttpGet("Test-Value")]
        public IActionResult TestValue()
        {
            string a = "1111111111111111111111111";
            return Ok(a);
        }
        [HttpGet("Get-Information")]
        public async Task<IActionResult> GetInformation()
        {
            ClaimsIdentity? claimsIdentity = User.Identity as ClaimsIdentity;
            string userName = claimsIdentity.Name.ToString();
            var result = await _userService.GetInformation(userName);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpGet("enable-2fa")]
        public async Task<IActionResult> EnableTwoFactorAuthentication()
        {
            ClaimsIdentity? claimsIdentity = User.Identity as ClaimsIdentity;
            string userName = claimsIdentity.Name.ToString();
            var result = await _userService.GetTwoFactorAuthenticationCode(userName);
            if (result.Success)
            {
                return Ok(result);
            }
            else
                return BadRequest(result);
        }
        [HttpPost("enable-2fa")]
        public async Task<IActionResult> EnableTwoFactorAuthentication([FromBody] EnableTwoFaReq passcode)
        {
            ClaimsIdentity? claimsIdentity = User.Identity as ClaimsIdentity;
            string userName = claimsIdentity.Name.ToString();
            var result = await _userService.TwoFactorAuthentication(userName, passcode);
            if (result.Success)
            {
                return Ok(result);
            }
            else
                return BadRequest(result);
        }

        [HttpPost("Disable-2fa")]
        public async Task<IActionResult> Disable2Fa([FromBody] EnableTwoFaReq passcode)
        {
            ClaimsIdentity? claimsIdentity = User.Identity as ClaimsIdentity;
            string userName = claimsIdentity.Name.ToString();
            var result = await _userService.Disable2FA(userName, passcode);
            if (result.Success)
            {
                return Ok(result);
            }
            else
                return BadRequest(result);
        }
        [HttpGet("Change-email")]
        public async Task<IActionResult> ChangeEmail()
        {
            ClaimsIdentity? claimsIdentity = User.Identity as ClaimsIdentity;
            string userName = claimsIdentity.Name.ToString();
            var result = await _userService.ChangeEmail(userName);
            if (result.Success == false)
            {
                return BadRequest(result);
            }
            var token = await _userManager.GenerateUserTokenAsync(result.Data, TokenOptions.DefaultPhoneProvider, "veriyOtp");
            await SendEmailAsync(result.Data.Email, "Change your Email",
           $"Your Change Email OTP : {token}");
            return Ok(new Result<string>()
            {
                Success=result.Success,
                Message=result.Message
            });
        }
        [HttpPost("Verify-Otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] EnterOtpReq req)
        {
            ClaimsIdentity? claimsIdentity = User.Identity as ClaimsIdentity;
            string userName = claimsIdentity.Name.ToString();
            var result = await _userService.VerifyOtp(userName, req.Otp);
            if (result.Success == false)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpPost("Change-Email")]
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailReq req)
        {
            ClaimsIdentity? claimsIdentity = User.Identity as ClaimsIdentity;
            string userName = claimsIdentity.Name.ToString();
            var result = await _userService.ChangeEmail(req,userName);
            if (!result.Success)
            {
                return BadRequest(result);
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
