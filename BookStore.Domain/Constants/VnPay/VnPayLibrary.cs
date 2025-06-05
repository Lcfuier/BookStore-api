using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Domain.Constants.VnPay
{
    public class VnPayLibrary
    {
        public const string VERSION = "2.1.0";
        private SortedList<String, String> _requestData = new SortedList<String, String>(new VnPayCompare());
        private SortedList<String, String> _responseData = new SortedList<String, String>(new VnPayCompare());

        public void AddRequestData(string key, string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                _responseData.Add(key, value);
            }
        }

        public string GetResponseData(string key)
        {
            string retValue;
            if (_responseData.TryGetValue(key, out retValue))
            {
                return retValue;
            }
            else
            {
                return string.Empty;
            }
        }

        #region Request

        public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
        {
            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in _requestData)
            {
                if (!String.IsNullOrEmpty(kv.Value))
                {
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }
            string queryString = data.ToString();

            baseUrl += "?" + queryString;
            String signData = queryString;
            if (signData.Length > 0)
            {

                signData = signData.Remove(data.Length - 1, 1);
            }
            string vnp_SecureHash = Utils.HmacSHA512(vnp_HashSecret, signData);
            baseUrl += "vnp_SecureHash=" + vnp_SecureHash;

            return baseUrl;
        }

        public string CreateRequestUrlToken(string baseUrl, string vnp_HashSecret)
        {
            // Bước 1: Sắp xếp tham số theo key (tên tham số)
            var sortedParams = _requestData
                .Where(kv => !string.IsNullOrEmpty(kv.Value))
                .OrderBy(kv => kv.Key, StringComparer.Ordinal);

            // Bước 2: Tạo query string (đã encode)
            StringBuilder data = new StringBuilder();
            foreach (var kv in sortedParams)
            {
                data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
            }

            // Bước 3: Xóa dấu & cuối cùng
            string queryString = data.ToString().TrimEnd('&');

            // Bước 4: Tạo chữ ký dựa trên queryString chưa encode (hoặc encode tùy yêu cầu VNPay)
            // VNPay yêu cầu dùng chuỗi queryString chưa encode để tạo chữ ký, nếu không có tài liệu rõ ràng thì cứ giữ nguyên
            string signData = queryString;

            // Bước 5: Tạo chữ ký HMAC SHA512
            string vnp_SecureHash = Utils.HmacSHA512(vnp_HashSecret, signData);

            // Bước 6: Tạo URL hoàn chỉnh (thêm chữ ký)
            string url = baseUrl + "?" + queryString + "&vnp_secure_hash=" + vnp_SecureHash;

            return url;
        }


        #endregion

        #region Response process

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            string rspRaw = GetResponseData();
            string myChecksum = Utils.HmacSHA512(secretKey, rspRaw);
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }
        private string GetResponseData()
        {

            StringBuilder data = new StringBuilder();
            if (_responseData.ContainsKey("vnp_SecureHashType"))
            {
                _responseData.Remove("vnp_SecureHashType");
            }
            if (_responseData.ContainsKey("vnp_SecureHash"))
            {
                _responseData.Remove("vnp_SecureHash");
            }
            if (_responseData.ContainsKey("vnp_secure_hash"))
            {
                _responseData.Remove("vnp_secure_hash");
            }
            foreach (KeyValuePair<string, string> kv in _responseData)
            {
                if (!String.IsNullOrEmpty(kv.Value))
                {
                    data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
                }
            }
            //remove last '&'
            if (data.Length > 0)
            {
                data.Remove(data.Length - 1, 1);
            }
            return data.ToString();
        }

        #endregion
    }

    public class Utils
    {


        public static String HmacSHA512(string key, String inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }

            return hash.ToString();
        }
        public static string GetIpAddress(HttpContext context)
        {
            try
            {
                var ip = context.Connection?.RemoteIpAddress;

                if (ip == null)
                    return "127.0.0.1";

                if (ip.IsIPv4MappedToIPv6)
                {
                    ip = ip.MapToIPv4();
                }

                if (IPAddress.IsLoopback(ip) || ip.ToString() == "::1")
                {
                    return "127.0.0.1";
                }

                return ip.ToString();
            }
            catch
            {
                return "127.0.0.1"; // fallback IP để tránh lỗi format
            }
        }


    }

    public class VnPayCompare : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            var vnpCompare = CompareInfo.GetCompareInfo("en-US");
            return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
        }
    }
}
