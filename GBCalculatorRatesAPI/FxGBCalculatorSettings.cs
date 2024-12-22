namespace GBCalculatorRatesAPI;

using Azure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

public class FxGBCalculatorSettings
{
    private readonly ILogger _logger;

	public FxGBCalculatorSettings(ILoggerFactory loggerFactory)
	{
		_logger = loggerFactory.CreateLogger<FxGBCalculatorSettings>();
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
		_logger.LogInformation("|| ** Processing calculator settings request.");

		var response = req.CreateResponse();

		return response;

	}
}