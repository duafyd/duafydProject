using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using UpBot.Models.Api;
using UpBot.Services.Apis;

namespace UpBot.Services
{
    public class ApiService
    {
        private readonly HttpClient _client = new();

        public QuotationApiClass QuotationApi { get; } = new();

        public async Task<T?> GetAsync<T>(string url, Dictionary<string, object>? queryParams)
        {
            try
            {
                var query = "";
                if (queryParams != null && queryParams.Count > 0)
                {
                    query = string.Join("&", queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value.ToString())}"));
                    url += url.Contains("?") ? "&" : "?";
                    url += query;
                }            

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Accept", "application/json");
                if (string.IsNullOrEmpty(query) == false)
                {
                    var jwt = CreateJwt(query);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
                }

                var response = await _client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (HttpRequestException ex)
            {
                return default;
            }
            catch (JsonException ex)
            {
                return default;
            }
            catch (Exception ex)
            {
                return default;
            }
        }

        public async Task<T?> PostAsync<T>(string url, Dictionary<string, object> body)
        {
            try
            {
                var jsonBody = JsonSerializer.Serialize(body);
                var queryStringBody = JsonToQueryString(jsonBody);
                var jwtPost = CreateJwt(queryStringBody);

                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");  
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtPost);

                var response = await _client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (HttpRequestException ex)
            {
                return default;
            }
            catch (JsonException ex)
            {
                return default;
            }
            catch (Exception ex)
            {
                return default;
            }
        }

        public static string JsonToQueryString(string json)
        {
            if (string.IsNullOrEmpty(json))
                return "";

            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            if (dict == null || dict.Count == 0) 
                return "";

            var query = new List<string>();
            foreach (var kv in dict)
            {
                if (kv.Value == null) continue;
                query.Add($"{HttpUtility.UrlEncode(kv.Key)}={HttpUtility.UrlEncode(kv.Value.ToString())}");
            }
            return string.Join("&", query);
        }

        private (string, string) GetKey()
        {
            var accessKey = "";
            var secretKey = "";

            var paht = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "key.txt");
            if (File.Exists(paht) == false)
                throw new FileNotFoundException("인증키 파일이 존재하지 않습니다.");

            var read = File.ReadAllText(paht);
            var json = JsonSerializer.Deserialize<Dictionary<string, object>>(read);

            accessKey = json?["accessKey"]?.ToString() ?? "";
            secretKey = json?["secretKey"]?.ToString() ?? "";

            return (accessKey, secretKey);
        }

        private string Sha512(string input)
        {
            using var sha = SHA512.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            var result = BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
            return result;
        }

        private string CreateJwt(string queryString)
        {
            var (accessKey, secretKey) = GetKey();

            var header = new Dictionary<string, object>
            {
                { "alg", "HS512" },
                { "typ", "JWT" }
            };

            var payload = new Dictionary<string, object>
            {
                { "access_key", accessKey },
                { "nonce", Guid.NewGuid().ToString() }
            };

            if (!string.IsNullOrEmpty(queryString))
            {
                var queryHash = Sha512(queryString);
                payload.Add("query_hash", queryHash);
                payload.Add("query_hash_alg", "SHA512");
            }

            var headerJson = JsonSerializer.Serialize(header);
            var payloadJson = JsonSerializer.Serialize(payload);

            var headerEncoded = Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson));
            var payloadEncoded = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));

            var unsignedToken = $"{headerEncoded}.{payloadEncoded}";
            var signature = ComputeHmacSha512(unsignedToken, secretKey);

            return $"{unsignedToken}.{signature}";
        }

        private string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private string ComputeHmacSha512(string data, string secret)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            using var hmac = new HMACSHA512(keyBytes);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Base64UrlEncode(hash);
        }
    }
}