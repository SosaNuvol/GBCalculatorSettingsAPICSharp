namespace GBCalculatorRatesAPI.Services;

using Azure;
using GBCalculatorRatesAPI.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QUAD.DSM;

public class UPMAServices
{
	private readonly HttpClient _httpClient;

	private readonly IConfiguration _configuration;

	private readonly ILogger<UPMAServices> _logger;

	private readonly string _upmaUrl;

	public UPMAServices(HttpClient httpClient, IConfiguration configuration, ILogger<UPMAServices> logger)
	{
		_httpClient = httpClient;
		_configuration = configuration;
		_logger = logger;

		_upmaUrl = _configuration["UPMA_PUBLIC_RATES_URL"] ?? "https://api.upma.org/api/public/rates";
	}

	public async Task<T> GetRatesOldAsync<T>()
	{
		var response = await _httpClient.GetAsync(_upmaUrl);
		response.EnsureSuccessStatusCode();

		var jsonResponse = await response.Content.ReadAsStringAsync();
		var result = JsonConvert.DeserializeObject<T>(jsonResponse);
		if (result == null)
		{
			throw new Exception("Response from UPMA API returned an error or a null.");
		}
		return result;
	}

	public async Task<DSMEnvelop<UPMAPayload, UPMAServices>> GetRatesAsync()
	{
		var response = DSMEnvelop<UPMAPayload, UPMAServices>.Initialize(_logger);

		try
		{
			var payload = await GetRatesOldAsync<UPMAPayload>();
			response.Success(payload);
			
		} catch (Exception ex) {
			response.Error(ex);
		}

		return response;
	}

	public async Task<DSMEnvelop<decimal, UPMAServices>> GetOfficialPrice() {
		var response = DSMEnvelop<decimal, UPMAServices>.Initialize(_logger);

		try
		{
			var rates = await GetRatesAsync();
			if (rates.Code != DSMEnvelopeCodeEnum._SUCCESS) response.Rebase(rates);

			if (!decimal.TryParse(rates.Payload.GoldbackOfficialPrice.Replace("$", string.Empty), out var officialPrice))
				return response.Error(DSMEnvelopeCodeEnum.API_SERVICES_06001, $"Unable to convert official price of \"{rates.Payload.GoldbackOfficialPrice}\"");

			response.Success(officialPrice);

		} catch (Exception ex) {
			response.Error(ex);
		}

		return response;
	}
}