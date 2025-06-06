using BookStore.Application.Interface;
using BookStore.Domain.DTOs;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Application.Services
{
    public class GhnService : IGhnService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public GhnService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<int> CalculateShippingFeeAsync(ShippingFeeRequest requestModel)
        {
            string urlBase = _configuration["Ghn:Url"];
            string apiKey = _configuration["Ghn:Apikey"];
            string shopId = _configuration["Ghn:ShopId"];
            var apiUrl = $"{urlBase}/shiip/public-api/v2/shipping-order/fee";

            var requestBody = new
            {
                from_district_id = 2023,
                from_ward_code = "371110",
                service_type_id = 2,
                to_district_id = requestModel.ToDistrictId,
                to_ward_code = requestModel.ToWardCode,
                weight = requestModel.Weight,
                length = requestModel.Length,
                width = requestModel.Width,
                height = requestModel.Height
            };

            var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
            request.Headers.Add("Token", apiKey);
            request.Headers.Add("ShopId", shopId);
            request.Content = new StringContent(JsonConvert.SerializeObject(requestBody), System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"GHN API lỗi: {content}");
            }

            var result = JsonConvert.DeserializeObject<dynamic>(content);
            if ((int?)result?.code != 200)
            {
                throw new Exception($"GHN API trả lỗi: {result?.message ?? "Không rõ lỗi"}");
            }

            return result.data.total;
        }
    }
}
