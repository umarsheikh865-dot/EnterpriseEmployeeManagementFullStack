using System.Net.Http;

namespace EnterpriseEmployeeManagement.Infrastructure.Services
{
    public class ExternalApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ExternalApiService(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> GetDataAsync(
            CancellationToken cancellationToken = default)
        {
            var client =
                _httpClientFactory.CreateClient("ExternalApi");

            var response =
                await client.GetAsync(
                    "https://jsonplaceholder.typicode.com/todos/1",
                    cancellationToken);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync(
                cancellationToken);
        }
    }
}
