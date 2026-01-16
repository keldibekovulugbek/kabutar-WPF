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
        private static ApiClient? _instance;
        private static readonly object _lock = new object();
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:5237/api/";  // Added trailing slash

        public static ApiClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ApiClient();
                        }
                    }
                }
                return _instance;
            }
        }

        private ApiClient()
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

                // Remove leading slash to work correctly with BaseAddress
                var relativeEndpoint = endpoint.TrimStart('/');
                var fullUrl = $"{BaseUrl}{relativeEndpoint}";
                Console.WriteLine($"[API] Sending POST to: {fullUrl}");
                Console.WriteLine($"[API] Request body: {json}");

                var response = await _httpClient.PostAsync(relativeEndpoint, content);

                Console.WriteLine($"[API] Response status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[API] Error response: {errorContent}");

                    var errorMessage = ParseErrorMessage(errorContent, (int)response.StatusCode);
                    throw new Exception(errorMessage);
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API] Response body: {responseJson}");

                return JsonConvert.DeserializeObject<TResponse>(responseJson);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[API] HTTP error: {ex.Message}");
                throw new Exception("Server bilan bog'lanishda xatolik. Internet aloqangizni tekshiring.", ex);
            }
            catch (Exception ex) when (ex.Message.StartsWith("Server bilan") || ex.Message.Contains("Email") || ex.Message.Contains("Parol") || ex.Message.Contains("Foydalanuvchi"))
            {
                // Already a user-friendly message, re-throw it
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API] Error: {ex.Message}");
                throw new Exception("Kutilmagan xatolik yuz berdi. Iltimos, qayta urinib ko'ring.", ex);
            }
        }

        public async Task<bool> PostAsync<TRequest>(string endpoint, TRequest data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Remove leading slash to work correctly with BaseAddress
                var relativeEndpoint = endpoint.TrimStart('/');
                var fullUrl = $"{BaseUrl}{relativeEndpoint}";
                Console.WriteLine($"[API] Sending POST to: {fullUrl}");
                Console.WriteLine($"[API] Request body: {json}");

                var response = await _httpClient.PostAsync(relativeEndpoint, content);

                Console.WriteLine($"[API] Response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                // Read error message from response
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API] Error response: {errorContent}");

                var errorMessage = ParseErrorMessage(errorContent, (int)response.StatusCode);
                throw new Exception(errorMessage);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[API] Network error: {ex.Message}");
                throw new Exception("Server bilan bog'lanishda xatolik. Internet aloqangizni tekshiring.", ex);
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"[API] Timeout error: {ex.Message}");
                throw new Exception("So'rov vaqti tugadi. Internet tezligingizni tekshiring.", ex);
            }
            catch (Exception ex) when (ex.Message.StartsWith("Server bilan") || ex.Message.Contains("Email") || ex.Message.Contains("Parol") || ex.Message.Contains("Foydalanuvchi"))
            {
                // Already a user-friendly message, re-throw it
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API] Unexpected error: {ex.Message}");
                throw new Exception("Kutilmagan xatolik yuz berdi. Iltimos, qayta urinib ko'ring.", ex);
            }
        }

        private string ParseErrorMessage(string errorContent, int statusCode)
        {
            try
            {
                // Try to parse as JSON error response
                var errorObj = JsonConvert.DeserializeObject<dynamic>(errorContent);

                // Backend custom error format: {"StatusCode": 404, "Message": "User not found."}
                if (errorObj?.Message != null)
                {
                    string message = errorObj.Message.ToString();

                    // Translate common error messages to Uzbek
                    if (message.Contains("User not found") || message.Contains("not found"))
                        return "Foydalanuvchi topilmadi. Email yoki username xato kiritilgan.";
                    if (message.Contains("Invalid password") || message.Contains("Incorrect password") || message.Contains("password is incorrect"))
                        return "Parol noto'g'ri. Iltimos, qayta urinib ko'ring.";
                    if (message.Contains("User already exists"))
                        return "Bu foydalanuvchi allaqachon ro'yxatdan o'tgan. Boshqa email yoki username tanlang.";
                    if (message.Contains("Email already exists"))
                        return "Bu email allaqachon ro'yxatdan o'tgan.";
                    if (message.Contains("Username already exists"))
                        return "Bu username allaqachon band.";

                    return message;
                }

                // Validation error format: {"errors": {"Password": ["Password must be..."]} }
                if (errorObj?.errors != null)
                {
                    var errors = new System.Collections.Generic.List<string>();

                    foreach (var prop in errorObj.errors)
                    {
                        string fieldName = prop.Name;
                        var messages = prop.Value;

                        foreach (var msg in messages)
                        {
                            string message = msg.ToString();

                            // Translate validation messages
                            if (message.Contains("Password must be"))
                                errors.Add("Parol 8-50 ta belgidan iborat bo'lishi va kamida 1 ta kichik, 1 ta katta harf hamda 1 ta raqam bo'lishi kerak.");
                            else if (message.Contains("Password is required") || message.Contains("Password cannot be empty"))
                                errors.Add("Parol kiritilishi shart.");
                            else if (message.Contains("Email is required") || message.Contains("Email cannot be empty"))
                                errors.Add("Email kiritilishi shart.");
                            else if (message.Contains("Email"))
                                errors.Add("Email noto'g'ri formatda.");
                            else if (fieldName == "Password")
                                errors.Add($"Parol: {message}");
                            else if (fieldName == "Email")
                                errors.Add($"Email: {message}");
                            else
                                errors.Add(message);
                        }
                    }

                    return errors.Count > 0 ? string.Join("\n", errors) : "Ma'lumotlar noto'g'ri to'ldirilgan.";
                }

                // If no specific error format found, return generic message based on status code
                return statusCode switch
                {
                    400 => "Ma'lumotlar noto'g'ri to'ldirilgan. Iltimos, qayta tekshiring.",
                    401 => "Login yoki parol noto'g'ri.",
                    404 => "Foydalanuvchi topilmadi. Email yoki username xato.",
                    500 => "Serverda xatolik yuz berdi. Iltimos, keyinroq urinib ko'ring.",
                    _ => "Xatolik yuz berdi. Iltimos, qayta urinib ko'ring."
                };
            }
            catch
            {
                // If parsing fails, return generic error based on status code
                return statusCode switch
                {
                    400 => "Ma'lumotlar noto'g'ri to'ldirilgan.",
                    401 => "Login yoki parol noto'g'ri.",
                    404 => "Foydalanuvchi topilmadi.",
                    _ => "Xatolik yuz berdi. Iltimos, qayta urinib ko'ring."
                };
            }
        }

        public async Task<TResponse?> GetAsync<TResponse>(string endpoint)
        {
            try
            {
                var relativeEndpoint = endpoint.TrimStart('/');
                var fullUrl = $"{BaseUrl}{relativeEndpoint}";
                Console.WriteLine($"[API] Sending GET to: {fullUrl}");

                var response = await _httpClient.GetAsync(relativeEndpoint);

                Console.WriteLine($"[API] Response status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[API] Error response: {errorContent}");

                    var errorMessage = ParseErrorMessage(errorContent, (int)response.StatusCode);
                    throw new Exception(errorMessage);
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[API] Response body: {responseJson}");

                return JsonConvert.DeserializeObject<TResponse>(responseJson);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[API] HTTP error: {ex.Message}");
                throw new Exception("Server bilan bog'lanishda xatolik. Internet aloqangizni tekshiring.", ex);
            }
            catch (Exception ex) when (ex.Message.StartsWith("Server bilan") || ex.Message.Contains("Email") || ex.Message.Contains("Parol") || ex.Message.Contains("Foydalanuvchi"))
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API] Error: {ex.Message}");
                throw new Exception("Kutilmagan xatolik yuz berdi. Iltimos, qayta urinib ko'ring.", ex);
            }
        }
    }
}
