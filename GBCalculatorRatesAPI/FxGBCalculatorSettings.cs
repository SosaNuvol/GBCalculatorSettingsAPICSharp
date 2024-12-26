namespace GBCalculatorRatesAPI;

using GBCalculatorRatesAPI.Business;
using GBCalculatorRatesAPI.Models;
using GBCalculatorRatesAPI.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public class FxGBCalculatorSettings
{
    private readonly ILogger _logger;

	private readonly GoogleServices _googleServices;
	
	private readonly LocationFacade _locationFacade;

	private readonly TransactionsFacade _transactionsFacade;

	private readonly RateChangeFacade _rateChangeFacade;

	private readonly CacheFacade<AppSettingsModel> _appSettingsCacheFacade;

	public FxGBCalculatorSettings(ILoggerFactory loggerFactory, GoogleServices googleServices, LocationFacade locationFacade, TransactionsFacade transactionsFacade, RateChangeFacade rateChangeFacade, CacheFacade<AppSettingsModel> appSettingsCacheFacade)
	{
		_logger = loggerFactory.CreateLogger<FxGBCalculatorSettings>();

		_googleServices = googleServices;
		_locationFacade = locationFacade;
		_transactionsFacade = transactionsFacade;
		_rateChangeFacade = rateChangeFacade;
		_appSettingsCacheFacade = appSettingsCacheFacade;
	}


    [Function("CalculatorSettings")]
    public async Task<HttpResponseData> CalculatorSettings(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
    {
		_logger.LogInformation("|| ** Processing calculator settings request.");

		var response = req.CreateResponse();

		var payloadResponse = await _appSettingsCacheFacade.GetCacheItem();

		// Serialize the payloadResponse to JSON
		var jsonResponse = JsonConvert.SerializeObject(payloadResponse);

		// Set the content type to application/json
		response.Headers.Add("Content-Type", "application/json");

		// Write the JSON response to the response body
		await response.WriteStringAsync(jsonResponse);

		return response;
	}

    [Function("MapData")]
	public async Task<HttpResponseData> MapData(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
	{
		_logger.LogInformation($"|| ** Processing calculator MapData {req.Method} request.");

		var response = req.CreateResponse();

		// var payloadResponse = await _googleServices.getLocations();
		var payloadResponse = await _locationFacade.SyncLocations();

		// Serialize the payloadResponse to JSON
		var jsonResponse = JsonConvert.SerializeObject(payloadResponse);

		// Set the content type to application/json
		response.Headers.Add("Content-Type", "application/json");

		// Write the JSON response to the response body
		await response.WriteStringAsync(jsonResponse);

		return response;
	}

	[Function("Transactions")]
	public async Task<HttpResponseData> Transactions(
		[HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
	{
		_logger.LogInformation($"|| ** Processing calculator Transactions {req.Method} request.");

		var response = req.CreateResponse();

		var payloadResponse = await _transactionsFacade.GetAllTransactions();

        // Serialize the payloadResponse to JSON
		var jsonResponse = JsonConvert.SerializeObject(payloadResponse);

        // Set the content type to application/json
        response.Headers.Add("Content-Type", "application/json");

        // Write the JSON response to the response body
        await response.WriteStringAsync(jsonResponse);
		
		return response;
	}

	[Function("RateChanges")]
	public async Task<HttpResponseData> RateChanges(
		[HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
	{
		_logger.LogInformation($"|| ** Processing calculator RateChanges {req.Method} request.");

		var response = req.CreateResponse();

		var payloadResponse = await _rateChangeFacade.GetAllRateChanges();

        // Serialize the payloadResponse to JSON
		var jsonResponse = JsonConvert.SerializeObject(payloadResponse);

        // Set the content type to application/json
        response.Headers.Add("Content-Type", "application/json");

        // Write the JSON response to the response body
        await response.WriteStringAsync(jsonResponse);
		
		return response;
	}
}