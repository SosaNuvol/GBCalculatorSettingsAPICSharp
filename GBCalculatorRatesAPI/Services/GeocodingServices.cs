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

		var result = new GBGeoCodes();

        if (json.RootElement.TryGetProperty("results", out var results) && results.GetArrayLength() > 0)
        {
            var location = results[0]
                .GetProperty("geometry")
                .GetProperty("location");

            var latitude = location.GetProperty("lat").GetDouble();
            var longitude = location.GetProperty("lng").GetDouble();

			result.Latitude = latitude;
			result.Longitude = longitude;
			result.Status = $"Set|{DateTimeOffset.Now}";

			if (results[0].TryGetProperty("formatted_address", out var formattedAddressProperty)) {
				result.FormattedAddress = formattedAddressProperty.GetString();
			}

			if (results[0].TryGetProperty("plus_code", out var plusCodeProperty)) {
				result.CompoundCode = plusCodeProperty.GetProperty("compound_code").GetString();
				result.GlobalCode = plusCodeProperty.GetProperty("global_code").GetString();
			}

            return result;
        }

		result.Status = "Address not found.";
		return result;
    }
}
