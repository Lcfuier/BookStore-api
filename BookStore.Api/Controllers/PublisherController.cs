using BookStore.Application.Interface;
using BookStore.Domain.Constants;
using BookStore.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookStore.Api.Controllers
{
    [Route("api/[controller]")]
    public class PublisherController : Controller
    {
        private readonly IPublisherService _publisherService;
        public PublisherController(IPublisherService publisherService)
        {
            _publisherService = publisherService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllPublisher(int page, int size, string? term)
        {
            var result = await _publisherService.GetAllPublishersAsync(page, size, term);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPublisherById(Guid id)
        {
            var result = await _publisherService.GetPublisherByIdAsync(id);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [Authorize(Roles = Roles.Admin + "," + Roles.Librarian)]
        [HttpPost]
        public async Task<IActionResult> AddPublisher([FromBody] AddPublisherReq param)
        {
            ClaimsIdentity? claimsIdentity = User.Identity as ClaimsIdentity;
            string userName = claimsIdentity.Name.ToString();
            var result = await _publisherService.AddPublisherAsync(param, userName);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [Authorize(Roles = Roles.Admin + "," + Roles.Librarian)]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdatePublisher([FromBody] UpdatePublisherReq param)
        {
            ClaimsIdentity? claimsIdentity = User.Identity as ClaimsIdentity;
            string userName = claimsIdentity.Name.ToString();
            var result = await _publisherService.UpdatePublisherAsync(param, userName);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [Authorize(Roles = Roles.Admin + "," + Roles.Librarian)]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeletePublisher(Guid id)
        {
            ClaimsIdentity? claimsIdentity = User.Identity as ClaimsIdentity;
            string userName = claimsIdentity.Name.ToString();
            var result = await _publisherService.RemovePublisherAsync(id, userName);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
