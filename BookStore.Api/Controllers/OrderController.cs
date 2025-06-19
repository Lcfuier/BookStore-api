using BookStore.Application.Interface;
using BookStore.Application.Services;
using BookStore.Domain.Constants;
using BookStore.Domain.Constants.VnPay;
using BookStore.Domain.DTOs;
using BookStore.Domain.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace BookStore.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : Controller
    {
       
        private readonly IOrderService _orderService;
        private readonly IVnPayService _vnPayService;
        private readonly IConfiguration _configuration;

        public OrderController(IOrderService orderService, IVnPayService vnPayService, IConfiguration configuration)
        {
            _orderService = orderService;
            _vnPayService = vnPayService;
            _configuration = configuration;
        }
        [HttpPost("checkout")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = Roles.User)]
        public async Task<IActionResult> Checkout([FromBody] OrderReq req)
        {
            ClaimsIdentity? claimsIdentity = User.Identity as ClaimsIdentity;
            string userName = claimsIdentity.Name.ToString();
            var result = await _orderService.Checkout(req,userName);
            var resultUrl = new Result<string>();
            resultUrl.Success = true;
            if (!result.Success)
            {
                return BadRequest(result);
            }
            else if (result.Success && result.Data.PaymentMethod.ToLower().Equals(PaymentMethod.PaymentMethodCash.ToLower()))
            {
                resultUrl.Data = _configuration["RedirectUrl:success"] + $"{result.Data.OrderId}";
                return Ok(resultUrl);
            }
            var vnpayRequest = new VnPayRequestModel()
            {
                Amount = result.Data.Total,
                OrderId = result.Data.OrderId.ToString(),
                Description = result.Data.OrderId.ToString(),
                StoreCard=req.IsSaveToken
            };
            string url="";
            if (req.IsUseToken && !string.IsNullOrEmpty(req.NickName))
            {
                
                url = await _vnPayService.CreateTokenPaymentUrl(HttpContext, vnpayRequest, userName,req.NickName);
                resultUrl.Data = url;
                return Ok(resultUrl);
            }
            if(req.IsSaveToken&&!string.IsNullOrEmpty(req.NickName))
            {
                url = _vnPayService.CreatePaymentUrl(HttpContext, vnpayRequest, userName,req.NickName);
            }
            else
            {
                url = _vnPayService.CreatePaymentUrl(HttpContext, vnpayRequest, userName,"");
            }
            resultUrl.Data = url;
            return Ok(resultUrl);
        }
        [AllowAnonymous]
        [HttpGet("vnpay-return")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PaymentCallBack()
        {
            
            VnPayResponeModel? vnpayResponse = await _vnPayService.PaymentExecuteAsync(Request.Query);
            if (vnpayResponse == null)
            {
                return Redirect(_configuration["RedirectUrl:invalid"]);
            }
            if (!vnpayResponse.paymentStatus.Equals("00"))
            {
                return Redirect(_configuration["RedirectUrl:error"]);
            }
            var result = await _orderService.VnPayCheckoutUpdate(Guid.Parse(vnpayResponse.BookingId), vnpayResponse, vnpayResponse.userName);

            if (!result.Success)
            {
                return Redirect(_configuration["RedirectUrl:failed"]);
            }

            return Redirect(_configuration["RedirectUrl:success"] +$"{result.Data}");
        }
        [HttpGet("user/orders")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize]
        public async Task<IActionResult> GetOrderById(int page,int size,string status,string orderCode)
        {
            ClaimsIdentity? claimsIdentity = User.Identity as ClaimsIdentity;
            string userName = claimsIdentity.Name.ToString();
            var result = await _orderService.GetOrderByUserNameAsync(page,size,userName,status,orderCode);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> GetOrderDeltailById(Guid id)
        {
            ClaimsIdentity? claimsIdentity = User.Identity as ClaimsIdentity;
            string userName = claimsIdentity.Name.ToString();
            var result = await _orderService.GetOrderByIdAsync(id,userName);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpPut("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> UpdateOrder(Guid id,[FromBody]UpdateOrderReq req)
        {
            ClaimsIdentity? claimsIdentity = User.Identity as ClaimsIdentity;
            string userName = claimsIdentity.Name.ToString();
            var result = await _orderService.UpdateOrderAsyc(userName,req,id);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
