using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GBCalculatorRatesAPI.Services
{
    public class UPMAServices
    {
        private readonly HttpClient _httpClient;

		public UPMAServices(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<T> GetRatesAsync<T>()
        {
            var response = await _httpClient.GetAsync("https://api.upma.org/api/public/rates");
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(jsonResponse);
        }
    }
}