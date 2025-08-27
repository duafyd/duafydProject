using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using UpBot.Models.Api;
using UpBot.Services.Apis;

namespace UpBot.Services
{
    public class ApiService
    {
        private readonly HttpClient _client = new();

        public QuotationApiClass QuotationApi { get; } = new();

        public async Task<T?> GetAsync<T>(string url, Dictionary<string, object>? queryParams = null)
        {
            try
            {
                if (queryParams != null && queryParams.Count > 0)
                {
                    var query = string.Join("&", queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value.ToString())}"));
                    url += url.Contains("?") ? "&" : "?";
                    url += query;
                }

                var response = await _client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (HttpRequestException)
            {
                return default;
            }
            catch (JsonException)
            {
                return default;
            }
            catch (Exception)
            {
                return default;
            }
        }

        public async Task<List<Market>?> GetUpbitMarketsAsync()
        {
            const string url = "https://api.upbit.com/v1/market/all";
            return await GetAsync<List<Market>>(url);
        }
        // POST, PUT 등 필요한 메서드 추가

      
    }
}