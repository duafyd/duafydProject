using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace UpBot.Services
{
    public class ApiService
    {
        private readonly HttpClient _client = new();

        public async Task<T?> GetAsync<T>(string url)
        {
            try
            {
                var response = await _client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (HttpRequestException ex)
            {
                // 네트워크 또는 HTTP 오류 처리
                // 필요에 따라 로그 남기기
                return default;
            }
            catch (JsonException ex)
            {
                // JSON 파싱 오류 처리
                return default;
            }
            catch (Exception ex)
            {
                // 기타 예외 처리
                return default;
            }
        }

        // POST, PUT 등 필요한 메서드 추가
    }
}