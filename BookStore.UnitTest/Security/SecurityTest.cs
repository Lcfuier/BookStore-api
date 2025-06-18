using BookStore.Domain.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.UnitTest.Security
{
    public class SecurityTest : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public SecurityTest(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Authentication_WithInvalidCredentials_ShouldReturnUnauthorized()
        {
            var invalidLoginRequest = new LoginReq
            {
                UserName = "nonexistent@example.com",
                Password = "WrongPassword123!"
            };

            var response = await _client.PostAsJsonAsync("/api/authenticate/login", invalidLoginRequest);

            // Log lỗi để debug
            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new Exception($"Server returned 500: {content}");
            }

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }


}
