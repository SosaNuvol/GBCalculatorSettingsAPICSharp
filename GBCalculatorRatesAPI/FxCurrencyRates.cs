namespace GBCalculatorRatesAPI;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GBCalculatorRatesAPI.Services;
using GBCalculatorRatesAPI.Models;
using System.Net;
using Newtonsoft.Json;
using Microsoft.Azure.Functions.Worker.Http;
using GBCalculatorRatesAPI.Business;

public class FxCurrencyRates
{
	private readonly ILogger<FxCurrencyRates> _logger;

	private readonly UPMAServices _upmaService;

	private readonly RateChangeFacade _rateChangeFacade;

	public FxCurrencyRates(UPMAServices uPMAServices, RateChangeFacade rateChangeFacade, ILogger<FxCurrencyRates> logger)
	{
		_upmaService = uPMAServices;
		_rateChangeFacade = rateChangeFacade;
		_logger = logger;
	}

	[Function("CurrencyRates")]
	public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
	{
		_logger.LogInformation("C# HTTP trigger function processed a request.");

		var payload = await _upmaService.GetRatesOldAsync<GBPricesModel>();

		return new OkObjectResult(payload);
	}

    [Function("ChangeRates")]
    public async Task<HttpResponseData> ChangeRates(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        _logger.LogInformation("|| ** Processing change rates request.");

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var changeRatesRequest = JsonConvert.DeserializeObject<RateChangeRequest>(requestBody);

        if (changeRatesRequest == null)
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var response = req.CreateResponse(HttpStatusCode.Created);

		var actionResponse = await _rateChangeFacade.SaveChange(changeRatesRequest);
		if (actionResponse.Code != QUAD.DSM.DSMEnvelopeCodeEnum._SUCCESS) {
			response.StatusCode = HttpStatusCode.BadRequest;
			await response.WriteStringAsync(actionResponse.ErrorMessage ?? "An Error occurred.");
		} else {
			response.StatusCode = HttpStatusCode.Created;
	        await response.WriteAsJsonAsync(actionResponse.Payload);
		}

        return response;
    }
}