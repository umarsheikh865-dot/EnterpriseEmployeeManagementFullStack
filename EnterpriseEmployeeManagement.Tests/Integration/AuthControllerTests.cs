using System.Net;
using System.Net.Http.Json;
using EnterpriseEmployeeManagement.Application.DTOs.Authentication;

namespace EnterpriseEmployeeManagement.Tests.Integration
{
    public class AuthControllerTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AuthControllerTests(
            CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "doesnotexist@test.com",
                Password = "WrongPassword123!"
            };

            // Act
            var response =
                await _client.PostAsJsonAsync(
                    "/api/Auth/login",
                    request);

            // Assert
            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }

        [Fact]
        public async Task Profile_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            // Act
            var response =
                await _client.GetAsync(
                    "/api/Auth/profile");

            // Assert
            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }
        [Fact]
        public async Task AdminEndpoint_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            // Act
            var response =
                await _client.GetAsync(
                    "/api/Auth/admin");

            // Assert
            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }
        [Fact]
        public async Task Login_WithEmptyRequest_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "",
                Password = ""
            };

            // Act
            var response =
                await _client.PostAsJsonAsync(
                    "/api/Auth/login",
                    request);

            // Assert
            Assert.True(
                response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.Unauthorized);
        }

    }
}