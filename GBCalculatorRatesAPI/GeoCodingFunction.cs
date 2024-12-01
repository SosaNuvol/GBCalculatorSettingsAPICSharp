using GBCalculatorRatesAPI.Business;
using GBCalculatorRatesAPI.Models;
using GBCalculatorRatesAPI.Repos;
using Microsoft.AspNetCore.Authentication;
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
        _logger.LogInformation("|| ** Processing geocoding request.");

		switch(req.Method)
		{
			case "GET":
				return await HandleGet(req)
					?? req.CreateResponse(HttpStatusCode.BadRequest);
			case "POST":
				return await HandlePost(req)
					?? req.CreateResponse(HttpStatusCode.BadRequest);
			default:
				return req.CreateResponse(HttpStatusCode.BadRequest);
		}

    }

	private async Task<HttpResponseData?> HandlePost(HttpRequestData req)
	{
		var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
		var geocodeRequest = JsonConvert.DeserializeObject<GeocodeRequest>(requestBody);

		if (geocodeRequest == null)
		{
			return req.CreateResponse(HttpStatusCode.BadRequest);
		}

		var result = new List<Location>();

		if (string.IsNullOrEmpty(geocodeRequest.ZipCodes)) {
			result = await _locationFacade.GetLocationsWithinRadiusAsync(
				geocodeRequest.Latitude,
				geocodeRequest.Longitude,
				geocodeRequest.RadiusInMeters
			);
		} else {
			result = await _locationFacade.GetLocationsWithZipCodesAsync(geocodeRequest.ZipCodes);
		}


		// Create response
		var response = req.CreateResponse(HttpStatusCode.OK);
		response.Headers.Remove("Content-Type");
		response.Headers.Add("Content-Type", "application/json; charset=utf-8");
		var json = JsonConvert.SerializeObject(result);
		await response.WriteStringAsync(json);

		return response;
	}

	private async Task<HttpResponseData?> HandleGet(HttpRequestData req)
	{
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
	}

	private async Task<HttpResponseData?> DefaultAction(HttpRequestData req) {
        try
        {
			var result = await _locationFacade.GeoCodeAllLocations();

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
