namespace GBCalculatorRatesAPI.Services;

using System.Text.Json;
using GBCalculatorRatesAPI.Models;
using Microsoft.Extensions.Logging;

public class GeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

	private readonly ILogger<GeocodingService> _logger;


    public GeocodingService(HttpClient httpClient, string apiKey, ILogger<GeocodingService> logger)
    {
        _httpClient = httpClient;
        _apiKey = apiKey;
		_logger = logger;
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
		var rightNow = DateTimeOffset.Now;
		result.SetStatus($"Set|{rightNow}");

        if (json.RootElement.TryGetProperty("results", out var results) && results.GetArrayLength() > 0)
        {
            var location = results[0]
                .GetProperty("geometry")
                .GetProperty("location");

			BindAddressComponents(result, results[0]);

            var latitude = location.GetProperty("lat").GetDouble();
            var longitude = location.GetProperty("lng").GetDouble();

			result.Latitude = latitude;
			result.Longitude = longitude;

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

	public void BindAddressComponents(GBGeoCodes geoCodes, JsonElement location) {
		if (!location.TryGetProperty("address_components", out var addressCompList)) return;

		if (TryFindAddressComponent(addressCompList, "locality", out var city))
		{
			geoCodes.City = city;
		}
		if (TryFindAddressComponent(addressCompList, "postal_code", out var postalCode))
		{
			geoCodes.PostalCode = postalCode;
		}
		if (TryFindAddressComponent(addressCompList, "administrative_area_level_1", out var stateProv))
		{
			geoCodes.StateProv = stateProv;
		}
		if (TryFindAddressComponent(addressCompList, "administrative_area_level_2", out var county))
		{
			geoCodes.County = county;
		}
		if (TryFindAddressComponent(addressCompList, "country", out var country))
		{
			geoCodes.Country = country;
		}
		if (TryFindAddressComponent(addressCompList, "street_number", out var houseNumber))
		{
			geoCodes.HouseNumber = houseNumber;
		}

	}

	public bool TryFindAddressComponent(JsonElement addressCompList, string componentName, out string value) {
		// var foundItem = addressCompList
		value = string.Empty;

		if (addressCompList.ValueKind != JsonValueKind.Array) return false;

		foreach(JsonElement element in addressCompList.EnumerateArray()) {
			if (!element.TryGetProperty("types", out var typesList)) continue;

			foreach(var type in typesList.EnumerateArray()) {
				var typeValue = type.GetString();
				if (!string.IsNullOrEmpty(typeValue) && typeValue.Equals(componentName)) {

					value = element.GetProperty("long_name").GetString() ?? string.Empty;
					return true;
				}
			}
		}

		return false;
	}
}
