using GBCalculatorRatesAPI.Business;
using GBCalculatorRatesAPI.Models;
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
	private readonly RateChangeFacade _rateChangeFacade;

    public GeocodeFunction(ILoggerFactory loggerFactory, LocationFacade locationFacade, RateChangeFacade rateChangeFacade)
    {
        _logger = loggerFactory.CreateLogger<GeocodeFunction>();
		_locationFacade = locationFacade;
		_rateChangeFacade = rateChangeFacade;
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
				return await HandlePostFindLocations(req)
					?? req.CreateResponse(HttpStatusCode.BadRequest);
			default:
				return req.CreateResponse(HttpStatusCode.BadRequest);
		}

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
		if (actionResponse.Code != QUAD.DSM.DSMEnvelopeCodeEnum.GEN_COMMON_00000) {
			response.StatusCode = HttpStatusCode.BadRequest;
			await response.WriteStringAsync(actionResponse.ErrorMessage ?? "An Error occurred.");
		} else {
			response.StatusCode = HttpStatusCode.Created;
	        await response.WriteAsJsonAsync(actionResponse.Payload);
		}

        return response;
    }

	private async Task<HttpResponseData?> HandlePostFindLocations(HttpRequestData req)
	{
		var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
		var geocodeRequest = JsonConvert.DeserializeObject<GeocodeRequest>(requestBody);

		if (geocodeRequest == null)
		{
			return req.CreateResponse(HttpStatusCode.BadRequest);
		}

		List<LocationDbEntity>? result;

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
		if (result == null) {
			var badResponse = req.CreateResponse(HttpStatusCode.BadGateway);
			badResponse.Headers.Remove("Content-Type");
			badResponse.Headers.Add("Content-Type", "application/json; charset=utf-8");
			
			return badResponse;
		}

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
				return await DefaultActionGetAllLocations(req)
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

	private async Task<HttpResponseData?> DefaultActionGetAllLocations(HttpRequestData req) {
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