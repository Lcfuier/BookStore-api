using BookStore.Application.Interface;
using BookStore.Application.Services;
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
        public OrderController(IOrderService orderService, IVnPayService vnPayService)
        {
            _orderService = orderService;
            _vnPayService=vnPayService;
        }
        [HttpPost("checkout-vnpay")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize]
        public async Task<IActionResult> CheckoutVnpay([FromBody] OrderReq req)
        {
            ClaimsIdentity? claimsIdentity = User.Identity as ClaimsIdentity;
            string userName = claimsIdentity.Name.ToString();
            var result = await _orderService.Checkout(req,userName);
            
            
            if (!result.Success)
            {
                return BadRequest(result);
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
                return Ok(url);
            }
            if(req.IsSaveToken&&!string.IsNullOrEmpty(req.NickName))
            {
                url = _vnPayService.CreatePaymentUrl(HttpContext, vnpayRequest, userName,req.NickName);
            }
            else
            {
                url = _vnPayService.CreatePaymentUrl(HttpContext, vnpayRequest, userName,"");
            }
            return Ok(url);
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
                return BadRequest("can't create the response.");
            }
            if (!vnpayResponse.paymentStatus.Equals("00"))
            {
                return BadRequest("Error when payment");
            }
            var result = await _orderService.VnPayCheckoutUpdate(Guid.Parse(vnpayResponse.BookingId), vnpayResponse, vnpayResponse.userName);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok("Succcessful");
        }
    }
}
