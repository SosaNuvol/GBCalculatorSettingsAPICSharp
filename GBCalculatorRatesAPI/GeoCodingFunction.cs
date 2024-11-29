using GBCalculatorRatesAPI.Business;
using GBCalculatorRatesAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;

namespace GBCalculatorRatesAPI;

public class GeocodeFunction
{
    private readonly ILogger _logger;
	private readonly LocationFacade _locationFacade;

    public GeocodeFunction(ILoggerFactory loggerFactory, LocationFacade locationFacade)
    {
        _logger = loggerFactory.CreateLogger<GeocodeFunction>();
		_locationFacade = locationFacade;
    }

    [Function("GeocodeFunction")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
    {
        _logger.LogInformation("Processing geocoding request.");

        // Read the address from query or body
        var queryParams = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
		var action = queryParams["action"];

		switch(action) {
			case "dump":
				return await DumpAction(req)
					?? req.CreateResponse(HttpStatusCode.BadRequest);
			default:
				return await DefaultAction(req)
					?? req.CreateResponse(HttpStatusCode.BadRequest);
		}
    }

	private async Task<HttpResponseData?> DumpAction(HttpRequestData req) {
		try {
			var result = await _locationFacade.DumbExcell();

			return req.CreateResponse(HttpStatusCode.Accepted);
		}
		catch (Exception ex)
		{
			_logger.LogError($"Geocoding failed: {ex.Message}");
			var response = req.CreateResponse(HttpStatusCode.BadRequest);
			await response.WriteStringAsync($"Error: {ex.Message}");
			return response;
		}
		return req.CreateResponse(HttpStatusCode.BadRequest);
	}

	private async Task<HttpResponseData?> DefaultAction(HttpRequestData req) {
        try
        {
			var result = await _locationFacade.GetLocationsWithCoordinates();

            // Create response
            var response = req.CreateResponse(HttpStatusCode.OK);
			response.Headers.Remove("Content-Type");
			response.Headers.Add("Content-Type", "application/json; charset=utf-8");
			var json = JsonConvert.SerializeObject(result);
			await response.WriteStringAsync(json);

			return response;
		}
		catch (Exception ex)
		{
			_logger.LogError($"Geocoding failed: {ex.Message}");
			var response = req.CreateResponse(HttpStatusCode.BadRequest);
			await response.WriteStringAsync($"Error: {ex.Message}");
			return response;
		}
	}
}
