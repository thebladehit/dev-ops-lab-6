using System.Text;
using System.Text.Json;

namespace DevOpsProject.Shared.Clients
{
    public class HiveHttpClient
    {
        private readonly HttpClient _httpClient;

        public HiveHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string?> SendHiveControlCommandAsync(string scheme, string ip, int port, object payload)
        {
            var uriBuilder = new UriBuilder
            {
                Scheme = scheme,
                Host = ip,
                Port = port,
                Path = "api/control"
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(uriBuilder.Uri, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return null;
        }
    }
}
