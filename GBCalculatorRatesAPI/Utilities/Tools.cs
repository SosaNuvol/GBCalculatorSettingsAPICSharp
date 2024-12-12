namespace GBCalculatorRatesAPI.Utilities;

using System.Net;
using GBCalculatorRatesAPI.Models;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;

public static class Tools {

    public static string GetCacheType<T>() {
        return typeof(T).Name;
    }

	public static bool hasCityData(string requestString) {
		if (IsCommaDelimitedNumbers(requestString)) {
			return false;
		}
		return true;
		// return IsCityAndState(requestString);
	}

	public static bool IsCommaDelimitedNumbers(string input) {
		var parts = input.Split(',');
		foreach (var part in parts) {
			if (!double.TryParse(part.Trim(), out _)) {
				return false;
			}
		}
		return true;
	}

	public static bool IsCityAndState(string input) {
		var parts = input.Split(',');
		if (parts.Length != 2) {
			return false;
		}
		var city = parts[0].Trim();
		var state = parts[1].Trim();
		return !string.IsNullOrEmpty(city) && !string.IsNullOrEmpty(state);
	}

	public static async Task<GeoCodeRequest?> GetGeoCodeRequest(HttpRequestData req) {
		var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
		var geoCodeRequest = JsonConvert.DeserializeObject<GeoCodeRequest>(requestBody);

		return geoCodeRequest;
	}

	public static async Task<HttpResponseData> CreateHttpResponse<T>(HttpRequestData req, HttpStatusCode code, T payload) {
		var response = req.CreateResponse(HttpStatusCode.OK);
			response.Headers.Remove("Content-Type");
			response.Headers.Add("Content-Type", "application/json; charset=utf-8");
			var json = JsonConvert.SerializeObject(payload);
			await response.WriteStringAsync(json);

		return response;
	}
}