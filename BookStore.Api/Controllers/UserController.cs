using BookStore.Application.Interface;
using BookStore.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookStore.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : Controller
    {

        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
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
    }
}
