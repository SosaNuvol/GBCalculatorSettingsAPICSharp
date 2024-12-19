namespace GBCalculatorRatesAPI;

using GBCalculatorRatesAPI.Business;
using GBCalculatorRatesAPI.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QUAD.DSM;
using System.Net;

/// <summary>
/// This Function does a lot of stuff.  I need to explain
/// 
/// </summary>
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

	[Function("GeocodeFunctionV2")]
	public async Task<HttpResponseData> RunV2(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
	{
		_logger.LogInformation("|| ** Processing geocoding request V2.");
		var geocodeRequest = await Utilities.Tools.GetGeoCodeRequest(req);

		if (geocodeRequest == null || string.IsNullOrEmpty(geocodeRequest.ZipCodes))
		{
			return req.CreateResponse(HttpStatusCode.BadRequest);
		}

		try {

			if (Utilities.Tools.IsCommaDelimitedNumbers(geocodeRequest.ZipCodes)) {
				var result = await _locationFacade.GetLocationsWithZipCodesV2Async(geocodeRequest.ZipCodes);
				return await Utilities.Tools.CreateHttpResponse(req, HttpStatusCode.OK, result);
			}

			var result2 = await _locationFacade.GetLocationsWithCityDataV2Async(geocodeRequest.ZipCodes);
			return await Utilities.Tools.CreateHttpResponse(req, HttpStatusCode.OK, result2);

		} catch(Exception ex) {
			// Log the exception (you can use your preferred logging framework)
			_logger.LogError(ex, "An error occurred while processing the request.");

			// Create an error response
			var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
			errorResponse.Headers.Remove("Content-Type");
			errorResponse.Headers.Add("Content-Type", "application/json; charset=utf-8");
			var errorJson = JsonConvert.SerializeObject(new { error = "An error occurred while processing your request." });
			await errorResponse.WriteStringAsync(errorJson);

			return errorResponse;
		}
	}
	
	[Function("GeocodeFunctionV3")]
	public async Task<HttpResponseData> RunV3(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
	{
		_logger.LogInformation("|| ** Processing geocoding request V3.");

		return await HandlePostFindLocationsV3(req) ?? req.CreateResponse(HttpStatusCode.BadRequest);
	}

	[Function("DownloadData")]
	public async Task<HttpResponseData> DownloadData(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
	{
        _logger.LogInformation("|| ** Processing Download Data request.");

		var response = req.CreateResponse(HttpStatusCode.EarlyHints);

		return response;
	}

	private async Task<HttpResponseData?> HandlePostFindLocationsV3(HttpRequestData req)
	{
		var geocodeRequest = await Utilities.Tools.GetGeoCodeRequest(req);

		if (geocodeRequest == null)
		{
			return req.CreateResponse(HttpStatusCode.BadRequest);
		}

		DSMEnvelop<GeoSearchQueryResult, LocationFacade>? result;

		if (string.IsNullOrEmpty(geocodeRequest.ZipCodes)) {
			result = await _locationFacade.GetLocationsWithInRadiusV3Async(
				geocodeRequest.Latitude,
				geocodeRequest.Longitude,
				geocodeRequest.RadiusInMeters
			);
		} else {
			// TODO: I need to check if this is a list of Zipcodes or a City.
			if (Utilities.Tools.hasCityData(geocodeRequest.ZipCodes)) {
				result = await _locationFacade.GetLocationsWithCityDataV3Async(geocodeRequest.ZipCodes);
			} else {
				result = await _locationFacade.GetLocationsWithZipCodesV3Async(geocodeRequest.ZipCodes);
			}
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

	private async Task<HttpResponseData?> HandlePostFindLocations(HttpRequestData req)
	{
		var geocodeRequest = await Utilities.Tools.GetGeoCodeRequest(req);

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
			// TODO: I need to check if this is a list of Zipcodes or a City.
			if (Utilities.Tools.hasCityData(geocodeRequest.ZipCodes)) {
				result = await _locationFacade.GetLocationsWithCityDataAsync(geocodeRequest.ZipCodes);
			} else {
				result = await _locationFacade.GetLocationsWithZipCodesAsync(geocodeRequest.ZipCodes);
			}
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