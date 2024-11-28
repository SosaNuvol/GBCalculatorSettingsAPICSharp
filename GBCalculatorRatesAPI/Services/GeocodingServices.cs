namespace GBCalculatorRatesAPI.Services;

using System.Text.Json;
using GBCalculatorRatesAPI.Models;

public class GeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GeocodingService(HttpClient httpClient, string apiKey)
    {
        _httpClient = httpClient;
        _apiKey = apiKey;
    }

    public async Task<GBGeoCodes> GeocodeAsync(string address)
    {
        var encodedAddress = Uri.EscapeDataString(address);
        var requestUri = $"https://maps.googleapis.com/maps/api/geocode/json?address={encodedAddress}&key={_apiKey}";

        var response = await _httpClient.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);

        if (json.RootElement.TryGetProperty("results", out var results) && results.GetArrayLength() > 0)
        {
            var location = results[0]
                .GetProperty("geometry")
                .GetProperty("location");

            var latitude = location.GetProperty("lat").GetDouble();
            var longitude = location.GetProperty("lng").GetDouble();

            return new GBGeoCodes { Latitude = latitude, Longitude = longitude };
        }

        throw new Exception("Address not found.");
    }
}
