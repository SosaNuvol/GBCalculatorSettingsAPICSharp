using GBCalculatorRatesAPI.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Threading.Tasks;

namespace GBCalculatorRatesAPI;

public class GeocodeFunction
{
    private readonly ILogger _logger;
    private readonly GeocodingService _geocodingService;

    public GeocodeFunction(ILoggerFactory loggerFactory, GeocodingService geocodingService)
    {
        _logger = loggerFactory.CreateLogger<GeocodeFunction>();
        _geocodingService = geocodingService;
    }

    [Function("GeocodeFunction")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
    {
        _logger.LogInformation("Processing geocoding request.");

        // Read the address from query or body
        var queryParams = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        var address = queryParams["address"] ?? "1600 Amphitheatre Parkway, Mountain View, CA";

        // Perform geocoding
        try
        {
            var geoCodes = await _geocodingService.GeocodeAsync(address);

            // Create response
            var response = req.CreateResponse(HttpStatusCode.OK);
			response.Headers.Remove("Content-Type");
			response.Headers.Add("Content-Type", "application/json; charset=utf-8");
			var json = JsonConvert.SerializeObject(geoCodes);
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
