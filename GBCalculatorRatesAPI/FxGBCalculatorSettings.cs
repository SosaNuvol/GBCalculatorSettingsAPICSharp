namespace GBCalculatorRatesAPI;

using GBCalculatorRatesAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public class FxGBCalculatorSettings
{
    private readonly ILogger _logger;

	private readonly GoogleServices _googleServices;

	public FxGBCalculatorSettings(ILoggerFactory loggerFactory, GoogleServices googleServices)
	{
		_logger = loggerFactory.CreateLogger<FxGBCalculatorSettings>();

		_googleServices = googleServices;
	}


    [Function("CalculatorSettings")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
    {
		_logger.LogInformation("|| ** Processing calculator settings request.");

		var response = req.CreateResponse();

		return response;
	}

    [Function("MapData")]
	public async Task<HttpResponseData> MapData(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
	{
		_logger.LogInformation($"|| ** Processing calculator MapData {req.Method} request.");

		var response = req.CreateResponse();

		var payloadResponse = await _googleServices.getLocations();

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

		return response;
	}
}