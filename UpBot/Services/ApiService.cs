using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace UpBot.Services
{
    public class ApiService
    {
        private readonly HttpClient _client = new();

        public async Task<T> GetAsync<T>(string url)
        {
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json);
        }

        // POST, PUT 등 필요한 메서드 추가
    }
}