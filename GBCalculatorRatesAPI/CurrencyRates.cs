namespace GBCalculatorRatesAPI;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GBCalculatorRatesAPI.Services;
using GBCalculatorRatesAPI.Models;

public class CurrencyRates
{
	private readonly ILogger<CurrencyRates> _logger;

	public CurrencyRates(ILogger<CurrencyRates> logger)
	{
		_logger = logger;
	}

	[Function("CurrencyRates")]
	public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
	{
		_logger.LogInformation("C# HTTP trigger function processed a request.");

		var client = new HttpClient();
		var services = new UPMAServices(client);

		var payload = await services.GetRatesAsync<UPMAPayload>();

		return new OkObjectResult(payload);
	}
}