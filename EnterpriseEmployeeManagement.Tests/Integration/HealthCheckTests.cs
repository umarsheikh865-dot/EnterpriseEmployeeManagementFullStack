namespace EnterpriseEmployeeManagement.Tests.Integration
{
    public class HealthCheckTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public HealthCheckTests(
            CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task HealthEndpoint_ShouldReturnSuccess()
        {
            // Act
            var response =
                await _client.GetAsync("/health");

            // Assert
            Assert.True(
                response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task LiveHealthEndpoint_ShouldReturnSuccess()
        {
            // Act
            var response =
                await _client.GetAsync("/health/live");

            // Assert
            Assert.True(
                response.IsSuccessStatusCode);
        }
    }
}