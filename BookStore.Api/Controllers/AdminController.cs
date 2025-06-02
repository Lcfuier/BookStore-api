using BookStore.Application.Interface;
using BookStore.Application.Services;
using BookStore.Domain.Constants;
using BookStore.Domain.DTOs;
using BookStore.Domain.Models;
using BookStore.Domain.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookStore.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles =Roles.Admin)]
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IOrderService _orderService;
        public AdminController(IUserService userService, UserManager<ApplicationUser> userManager, IConfiguration configuration, IOrderService orderService)
        {
            _userService = userService;
            _userManager = userManager;
            _configuration = configuration;
            _orderService = orderService;

        }
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers(int page, int size, string? term)
        {
            var result = await _userService.GetAllUsersAsync(page, size, term);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserReq req,string id)
        {
            var result = await _userService.UpdateUser(id,req);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserInformation(string id)
        {
            var result = await _userService.GetUserInformationAsync(id);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpPost("books-sold-by-date")]
        [Authorize(Roles = Roles.Admin+","+Roles.Librarian)]
        public async Task<IActionResult> GetBooksSoldByDate([FromBody] DateFilter filter)
        {
            if (filter == null)
            {
                return BadRequest();
            }
            var result= await _orderService.GetBooksSoldByDate(filter);
            return Ok(result);
        }
        [HttpPost("revenue-by-date")]
        [Authorize(Roles = Roles.Admin + "," + Roles.Librarian)]
        public async Task<IActionResult> GetRevenueByDate([FromBody] DateFilter filter)
        {
            if (filter == null)
            {
                return BadRequest(new Result<string>
                {
                    Success = false,
                    Message = "Hãy chọn ngày bắt đầu và kết thúc!"
                });
            }
            var result = await _orderService.GetRevenueByDate(filter);
            return Ok(result);
        }
    }
}
