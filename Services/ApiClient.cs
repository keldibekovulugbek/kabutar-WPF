using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Kabutar_WPF.Services
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:5237/api";

        public ApiClient()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void SetAuthToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public void ClearAuthToken()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var fullUrl = $"{BaseUrl}{endpoint}";
                Console.WriteLine($"[API] Sending POST to: {fullUrl}");
                Console.WriteLine($"[API] Request body: {json}");

                var response = await _httpClient.PostAsync(endpoint, content);

                Console.WriteLine($"[API] Response status: {response.StatusCode}");

                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API] Response body: {responseJson}");

                return JsonConvert.DeserializeObject<TResponse>(responseJson);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[API] HTTP error: {ex.Message}");
                throw new Exception($"API request failed: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API] Error: {ex.Message}");
                throw new Exception($"Unexpected error: {ex.Message}", ex);
            }
        }

        public async Task<bool> PostAsync<TRequest>(string endpoint, TRequest data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var fullUrl = $"{BaseUrl}{endpoint}";
                Console.WriteLine($"[API] Sending POST to: {fullUrl}");
                Console.WriteLine($"[API] Request body: {json}");

                var response = await _httpClient.PostAsync(endpoint, content);

                Console.WriteLine($"[API] Response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                // Read error message from response
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API] Error response: {errorContent}");
                throw new Exception($"Server returned error: {response.StatusCode}. {errorContent}");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[API] Network error: {ex.Message}");
                throw new Exception($"Network error: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"[API] Timeout error: {ex.Message}");
                throw new Exception("Request timeout. Please check your internet connection.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API] Unexpected error: {ex.Message}");
                throw;
            }
        }

        public async Task<TResponse?> GetAsync<TResponse>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TResponse>(json);
            }
            catch
            {
                return default;
            }
        }
    }
}
