namespace GBCalculatorRatesAPI.Services;

using System.Text.Json;
using GBCalculatorRatesAPI.Models;
using Newtonsoft.Json;
using QUAD.DSM;

public class ExchangeServices {
	private readonly HttpClient _httpClient;
    private readonly string _apiKey;
	private const string URI = "https://api.exchangerate.host/live?access_key=3ab01cee0923aaf526a957a3b5ba8c31&format=1";

    public ExchangeServices(HttpClient httpClient, string apiKey)
    {
        _httpClient = httpClient;
        _apiKey = apiKey;
    }

	public async Task<DSMEnvelop<ExchangeRateModel>> GetExchangeRates()
	{
		var response = DSMEnvelop<ExchangeRateModel>.Initialize();

		try {
			var responseAction = await _httpClient.GetAsync(URI);
			responseAction.EnsureSuccessStatusCode();

			var content = await responseAction.Content.ReadAsStringAsync();

			var result = JsonConvert.DeserializeObject<ExchangeRateModel>(content);
			if (result == null) return response.Error(DSMEnvelopeCodeEnum.API_SERVICES_06001, "No content was returned when calling the currency exchange.");

			response.Success(result);

		} catch(Exception ex) {
			response.Error(DSMEnvelopeCodeEnum.API_SERVICES_06001, $"Error on Calling Exchange rate API: {ex.Message}");
		}

		return response;
	}
}