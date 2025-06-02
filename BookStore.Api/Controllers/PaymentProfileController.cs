using BookStore.Application.Interface;
using BookStore.Application.Services;
using BookStore.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookStore.Api.Controllers
{
    [Route("api/[controller]s")]
    [Authorize]
    public class PaymentProfileController : Controller
    {
        private readonly IPaymentProfileService _paymentProfileService;
        public PaymentProfileController(IPaymentProfileService paymentProfileService)
        {
            _paymentProfileService= paymentProfileService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllPaymentProfile()
        {
            ClaimsIdentity? claimsIdentity = User.Identity as ClaimsIdentity;
            string userName = claimsIdentity.Name.ToString();
            var result = await _paymentProfileService.GetAllPaymentProfileAsync(userName);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpGet("paymentProfile")]
        public async Task<IActionResult> GetPaymentProfilebyName(string name)
        {
            ClaimsIdentity? claimsIdentity = User.Identity as ClaimsIdentity;
            string userName = claimsIdentity.Name.ToString();
            var result = await _paymentProfileService.GetPaymentProfileByNameAsync(userName,name);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

      /*  [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateAuthor([FromBody] UpdateAuthorReq param)
        {
            ClaimsIdentity? claimsIdentity = User.Identity as ClaimsIdentity;
            string userName = claimsIdentity.Name.ToString();
            var result = await _authorService.UpdateAuthorAsync(param, userName);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }*/
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeletePaymentProfile(Guid id)
        {
            ClaimsIdentity? claimsIdentity = User.Identity as ClaimsIdentity;
            string userName = claimsIdentity.Name.ToString();
            var result = await _paymentProfileService.RemovePaymentProfileAsync(id);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
