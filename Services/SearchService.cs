using System;
using System.Threading.Tasks;
using System.Web;
using Kabutar_WPF.Models.Search;

namespace Kabutar_WPF.Services
{
    public interface ISearchService
    {
        Task<SearchResult?> SearchAsync(string query);
    }

    public class SearchService : ISearchService
    {
        private readonly ApiClient _apiClient;

        public SearchService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<SearchResult?> SearchAsync(string query)
        {
            try
            {
                var encodedQuery = HttpUtility.UrlEncode(query);
                var result = await _apiClient.GetAsync<SearchResult>($"/search?query={encodedQuery}");
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Qidiruv xatoligi: {ex.Message}", ex);
            }
        }
    }
}
