using System.Net;

namespace EnterpriseEmployeeManagement.Tests.Integration
{
    public class EmployeeControllerTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public EmployeeControllerTests(
            CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetEmployees_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            // Act
            var response =
                await _client.GetAsync(
                    "/api/Employee");

            // Assert
            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }

        [Fact]
        public async Task GetEmployeeById_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            // Act
            var response =
                await _client.GetAsync(
                    "/api/Employee/1");

            // Assert
            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }
    }
}