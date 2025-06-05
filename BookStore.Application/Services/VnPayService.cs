using AutoMapper;
using BookStore.Application.Interface;
using BookStore.Domain.Constants.VnPay;
using BookStore.Domain.DTOs;
using BookStore.Domain.Models;
using BookStore.Domain.Queries;
using BookStore.Infrastructure.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BookStore.Application.Services
{
    public class VnPayService : IVnPayService
    {

        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _data;
        private readonly UserManager<ApplicationUser> _userManager;
        public VnPayService(IConfiguration configuration, IUnitOfWork data, UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _data = data;   
            _userManager = userManager;
        }
        public string CreatePaymentUrl(HttpContext httpContext, VnPayRequestModel request, string userName,string? nickName)
        {
            var tick = DateTime.Now.Ticks.ToString();
            var vnpay = new VnPayLibrary();


            if (request.StoreCard == true)
            {
                string txnDesc = $"{request.OrderId}|{userName}|{nickName}|SaveToken";
                vnpay.AddRequestData("vnp_version", _configuration["VnPay:Version"]);
                vnpay.AddRequestData("vnp_command", "pay_and_create");
                vnpay.AddRequestData("vnp_tmn_code", _configuration["VnPay:TmnCode"]);
                vnpay.AddRequestData("vnp_app_user_id", userName);
                vnpay.AddRequestData("vnp_card_type", "01");
                vnpay.AddRequestData("vnp_txn_ref", tick);
                vnpay.AddRequestData("vnp_amount", ((int)(request.Amount * 100)).ToString());
                vnpay.AddRequestData("vnp_curr_code", _configuration["VnPay:CurrCode"]);
                vnpay.AddRequestData("vnp_txn_desc", txnDesc);
                vnpay.AddRequestData("vnp_return_url", _configuration["VnPay:PaymentBackReturnUrl"]);
                vnpay.AddRequestData("vnp_ip_addr", Utils.GetIpAddress(httpContext));
                vnpay.AddRequestData("vnp_create_date", DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss"));
                vnpay.AddRequestData("vnp_store_token", "1");
                vnpay.AddRequestData("vnp_locale", _configuration["VnPay:Locale"]);
                return vnpay.CreateRequestUrlToken(_configuration["VnPay:BaseUrlCreatenPayToken"], _configuration["VnPay:HashSecret"]);
            }
            else
            {
                //
                vnpay.AddRequestData("vnp_OrderInfo", $"{request.OrderId}|{userName}");
                vnpay.AddRequestData("vnp_Version", _configuration["VnPay:Version"]);
                vnpay.AddRequestData("vnp_Command", _configuration["VnPay:Command"]);
                vnpay.AddRequestData("vnp_TmnCode", _configuration["VnPay:TmnCode"]);
                vnpay.AddRequestData("vnp_Amount", ((int)(request.Amount * 100)).ToString());
                vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                vnpay.AddRequestData("vnp_CurrCode", _configuration["VnPay:CurrCode"]);
                vnpay.AddRequestData("vnp_Locale", _configuration["VnPay:Locale"]);
                vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(httpContext));
                vnpay.AddRequestData("vnp_OrderType", "other");
                vnpay.AddRequestData("vnp_TxnRef", tick);
                vnpay.AddRequestData("vnp_ReturnUrl", _configuration["VnPay:PaymentBackReturnUrl"]);
                return vnpay.CreateRequestUrl(_configuration["VnPay:BaseUrl"], _configuration["VnPay:HashSecret"]);
            }
            
        }

        public async Task<VnPayResponeModel> PaymentExecuteAsync(IQueryCollection collection)
        {
            var vnpay = new VnPayLibrary();
            foreach (var (key, value) in collection)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value.ToString());
                }
            }
            var storeCard = vnpay.GetResponseData("vnp_token");
            string orderId = "";
            string nickname = "";
            string userName = "";
            var responseCode = "";
            var transactionId = "";
            var orderInfoRaw = "";
            var txnRef = "";
            var cardToken = "";
            var paymentStatus = "";
            var secureHash = "";
            string saveToken = "";
            if (!string.IsNullOrEmpty(storeCard))
            {
                secureHash = collection.FirstOrDefault(p => p.Key == "vnp_secure_hash").Value;
                var checkSignature = vnpay.ValidateSignature(secureHash, _configuration["VnPay:HashSecret"]);
                if (!checkSignature)
                {
                    return new VnPayResponeModel { IsSuccess = false };
                }
                responseCode = vnpay.GetResponseData("vnp_response_code");
                transactionId = vnpay.GetResponseData("vnp_transaction_no");
                orderInfoRaw = vnpay.GetResponseData("vnp_txn_desc");
                txnRef = vnpay.GetResponseData("vnp_txn_ref");
                cardToken = vnpay.GetResponseData("vnp_token");
                paymentStatus = vnpay.GetResponseData("vnp_transaction_status");


                var rawDesc = vnpay.GetResponseData("vnp_txn_desc");

                if (!string.IsNullOrEmpty(rawDesc) && rawDesc.Contains("|"))
                {
                    var parts = rawDesc.Split('|');
                    orderId = parts.ElementAtOrDefault(0);
                    userName = parts.ElementAtOrDefault(1);
                    nickname = parts.ElementAtOrDefault(2);
                    saveToken = parts.ElementAtOrDefault(3);
                }
                var existing = await _userManager.FindByNameAsync(userName);
                if (existing != null && saveToken.Equals("SaveToken"))
                {
                    _data.PaymentProfile.Add(new PaymentProfile
                    {
                        User = existing,
                        Token = cardToken,
                        CreatedTime = DateTime.UtcNow,
                        Nickname = nickname,
                        CreatedBy = userName,
                    });
                    await _data.SaveAsync();
                }
            }
            else
            {
                secureHash = collection.FirstOrDefault(p => p.Key == "vnp_SecureHash").Value;
                var checkSignature = vnpay.ValidateSignature(secureHash, _configuration["VnPay:HashSecret"]);
                if (!checkSignature)
                {
                    return new VnPayResponeModel { IsSuccess = false };
                }
                responseCode = vnpay.GetResponseData("vnp_ResponseCode");
                transactionId = vnpay.GetResponseData("vnp_TransactionNo");
                orderInfoRaw = vnpay.GetResponseData("vnp_OrderInfo");
                txnRef = vnpay.GetResponseData("vnp_TxnRef");
                paymentStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                var rawDesc = orderInfoRaw;

                if (!string.IsNullOrEmpty(rawDesc) && rawDesc.Contains("|"))
                {
                    var parts = rawDesc.Split('|');
                    orderId = parts.ElementAtOrDefault(0);
                    userName = parts.ElementAtOrDefault(1);
                    nickname = parts.ElementAtOrDefault(2);
                    saveToken = parts.ElementAtOrDefault(3);
                }
            }

            return new VnPayResponeModel
            {
                IsSuccess = true,
                PaymentMethod = "VnPay",
                BookingId = orderId,
                TxnRef = txnRef,
                TransactionId = transactionId,
                Token = secureHash,
                VnPayResponeCode = responseCode,
                userName = userName,
                paymentStatus = paymentStatus,
            };
        }


        // Hàm decode base64 url-safe (thay thế - và _ và thêm padding)
        private string DecodeBase64UrlSafe(string base64Url)
        {
            string base64 = base64Url.Replace('-', '+').Replace('_', '/');
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            var bytes = Convert.FromBase64String(base64);
            return Encoding.UTF8.GetString(bytes);
        }


        public async Task<string> CreateTokenPaymentUrl(HttpContext httpContext, VnPayRequestModel req, string userName,string nickName)
        {
            var user=await _userManager.FindByNameAsync(userName);
            var paymentProfile = await _data.PaymentProfile.GetAsync(new QueryOptions<PaymentProfile>
            {
                Where= c=>c.UserId.Equals(user.Id) && c.Nickname.Equals(nickName)
            });
            if (string.IsNullOrEmpty(paymentProfile.Token))
            {
                throw new Exception("No saved token found for user.");
            }
            var tick = DateTime.Now.Ticks.ToString();
            var orderInfo =  $"{req.OrderId}|{userName}|{nickName}|false";
            var vnpay = new VnPayLibrary();
            vnpay.AddRequestData("vnp_version", _configuration["VnPay:Version"]);
            vnpay.AddRequestData("vnp_command", "token_pay");
            vnpay.AddRequestData("vnp_tmn_code", _configuration["VnPay:TmnCode"]);
            vnpay.AddRequestData("vnp_txn_ref", tick);
            vnpay.AddRequestData("vnp_app_user_id", userName);
            vnpay.AddRequestData("vnp_token", paymentProfile.Token);
            vnpay.AddRequestData("vnp_amount", ((int)(req.Amount * 100)).ToString());
            vnpay.AddRequestData("vnp_curr_code", _configuration["VnPay:CurrCode"]);
            vnpay.AddRequestData("vnp_txn_desc", orderInfo);
            vnpay.AddRequestData("vnp_return_url", _configuration["VnPay:PaymentBackReturnUrl"]);
            vnpay.AddRequestData("vnp_ip_addr", Utils.GetIpAddress(httpContext));
            vnpay.AddRequestData("vnp_create_date", DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_locale", _configuration["VnPay:Locale"]);
            return vnpay.CreateRequestUrlToken(_configuration["VnPay:BaseUrlPayToken"], _configuration["VnPay:HashSecret"]);
            
        }
    }
}
