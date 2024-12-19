namespace GBCalculatorRatesAPI;

using Azure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

public class CalculatorSettingsFunction
{
    private readonly ILogger _logger;

	public CalculatorSettingsFunction(ILoggerFactory loggerFactory)
	{
		_logger = loggerFactory.CreateLogger<CalculatorSettingsFunction>();
	}


    [Function("CalculatorSettings")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
    {
		_logger.LogInformation("|| ** Processing calculator settings request.");

		var response = req.CreateResponse();

		return response;
	}
}