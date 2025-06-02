using BookStore.Domain.DTOs;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BookStore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private const string GhnApiUrl = "https://api.giaohangnhanh.vn"; // GHN API base URL
        private readonly IConfiguration _configuration;
        public AddressController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration=configuration;
        }

        // API lấy danh sách tỉnh thành
        [HttpGet("provinces")]
        public async Task<IActionResult> GetProvinces()
        {
            try
            {
                string urlBase = _configuration["Ghn:Url"];
                string apiKey = _configuration["Ghn:Apikey"];
                var requestUrl = $"{urlBase}/shiip/public-api/master-data/province";

                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Add("Token", apiKey);

                var response = await _httpClient.SendAsync(request);

                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest($"GHN API lỗi: {content}");
                }

                var provincesResponse = JsonConvert.DeserializeObject<ProvinceResponse>(content);

                if (provincesResponse == null || provincesResponse.Code != 200)
                {
                    return BadRequest($"GHN API trả lỗi: {provincesResponse?.Message ?? "Không rõ lỗi"}");
                }

                // Trả ra ID + Name
                var provinces = provincesResponse.Data.Select(p => new
                {
                    Id = p.ProvinceID,
                    Name = p.ProvinceName
                }).ToList();

                return Ok(provinces);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đã xảy ra lỗi: {ex.Message}");
            }
        }



        // API lấy danh sách quận huyện theo tỉnh
        [HttpGet("districts/{provinceId}")]
        public async Task<IActionResult> GetDistricts(int provinceId)
        {
            try
            {
                string urlBase = _configuration["Ghn:Url"];
                string apiKey = _configuration["Ghn:Apikey"];
                var requestUrl = $"{urlBase}/shiip/public-api/master-data/district"; // đúng endpoint GHN mới

                var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                request.Headers.Add("Token", apiKey);

                // GHN yêu cầu POST body khi lấy quận
                var body = new { province_id = provinceId };
                request.Content = new StringContent(JsonConvert.SerializeObject(body), System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);

                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest($"GHN API lỗi: {content}");
                }

                var districtsResponse = JsonConvert.DeserializeObject<DistrictResponse>(content);

                if (districtsResponse == null || districtsResponse.Code != 200)
                {
                    return BadRequest($"GHN API trả lỗi: {districtsResponse?.Message ?? "Không rõ lỗi"}");
                }

                // Trả ra ID + Name
                var districts = districtsResponse.Data.Select(d => new
                {
                    Id = d.DistrictID,
                    Name = d.DistrictName
                }).ToList();

                return Ok(districts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đã xảy ra lỗi: {ex.Message}");
            }
        }

        // API lấy danh sách phường xã theo quận
        [HttpGet("wards/{districtId}")]
        public async Task<IActionResult> GetWards(int districtId)
        {
            try
            {
                string urlBase = _configuration["Ghn:Url"];
                string apiKey = _configuration["Ghn:Apikey"];
                var requestUrl = $"{urlBase}/shiip/public-api/master-data/ward";

                var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                request.Headers.Add("Token", apiKey);

                // Body truyền district_id
                var body = new { district_id = districtId };
                request.Content = new StringContent(JsonConvert.SerializeObject(body), System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);

                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest($"GHN API lỗi: {content}");
                }

                var wardsResponse = JsonConvert.DeserializeObject<WardResponse>(content);

                if (wardsResponse == null || wardsResponse.Code != 200)
                {
                    return BadRequest($"GHN API trả lỗi: {wardsResponse?.Message ?? "Không rõ lỗi"}");
                }

                // Trả ra ID + Name
                var wards = wardsResponse.Data.Select(w => new
                {
                    Id = w.WardCode,
                    Name = w.WardName
                }).ToList();

                return Ok(wards);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đã xảy ra lỗi: {ex.Message}");
            }
        }

    }

}
