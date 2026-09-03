namespace EnterpriseEmployeeManagement.Tests.Integration
{
    public class InfoControllerTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public InfoControllerTests(
            CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetInfo_ShouldReturnSuccess()
        {
            // Act
            var response =
                await _client.GetAsync("/api/Info");

            // Assert
            Assert.True(
                response.IsSuccessStatusCode);
        }
    }
}