namespace GBCalculatorRatesAPI.Business;

using System;
using System.Collections.Generic;
using ClosedXML.Excel;
using DnsClient.Internal;
using GBCalculatorRatesAPI.Business.Models;
using GBCalculatorRatesAPI.Models;
using GBCalculatorRatesAPI.Repos;
using GBCalculatorRatesAPI.Services;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using QUAD.DSM;

public class LocationFacade
{
	private readonly ILogger<LocationFacade> _logger;
	private readonly LocationsRepository _locationsRepository;
	private readonly GeocodingService _geocodingServices;

	private const double radiansConstant = 6378.1; // Radius in radians: radius in km / Earth's radius (approx. 6378.1 km)

	public LocationFacade(ILogger<LocationFacade> logger, LocationsRepository locationsRepository, GeocodingService geocodingServices)
	{
		_logger = logger;
		_locationsRepository = locationsRepository;
		_geocodingServices = geocodingServices;
	}

	public async Task<DSMEnvelop<GeoSearchQueryResult, LocationFacade>> GetLocationsWithInRadiusV3Async(double latitude, double longitude, double radiusInMeters)
	{
		var response = DSMEnvelop<GeoSearchQueryResult, LocationFacade>.Initialize(_logger);

		try 
		{
			var collection = _locationsRepository.LocationsCollection;

			var pipeline = new BsonDocument[]
			{
				new BsonDocument("$geoNear", new BsonDocument
				{
					{ "near", new BsonDocument("type", "Point").Add("coordinates", new BsonArray { longitude, latitude }) },
					{ "key", "location" },
					{ "distanceField", "distance" },
					{ "maxDistance", radiusInMeters },
					{ "spherical", true }
				}),
				new BsonDocument("$group", new BsonDocument
				{
					{ "_id", new BsonDocument { { "longitude", "$longitude" }, { "latitude", "$latitude" } } },
					{ "count", new BsonDocument("$sum", 1) },
					{ "documents", new BsonDocument("$push", "$$ROOT") }
				}),
				new BsonDocument("$project", new BsonDocument
				{
					{ "_id", 0 },
					{ "longitude", "$_id.longitude" },
					{ "latitude", "$_id.latitude" },
					{ "count", 1 },
					{ "documents", 1 },
					{ "isDuplicate", new BsonDocument("$gt", new BsonArray { "$count", 1 }) }
				}),
				new BsonDocument("$group", new BsonDocument
				{
					{ "_id", BsonNull.Value },
					{ "groupedLocations", new BsonDocument("$push", new BsonDocument("$cond", new BsonArray
					{
						new BsonDocument("$eq", new BsonArray { "$isDuplicate", true }),
						new BsonDocument
						{
							{ "longitude", "$longitude" },
							{ "latitude", "$latitude" },
							{ "count", "$count" },
							{ "documents", "$documents" }
						},
						new BsonString("$$REMOVE")
					})) },
					{ "singleLocations", new BsonDocument("$push", new BsonDocument("$cond", new BsonArray
					{
						new BsonDocument("$eq", new BsonArray { "$isDuplicate", false }),
						new BsonDocument("$arrayElemAt", new BsonArray { "$documents", 0 }),
						new BsonString("$$REMOVE")
					})) }
				}),
				new BsonDocument("$project", new BsonDocument
				{
					{ "_id", 0 },
					{ "groupedLocations", 1 },
					{ "singleLocations", 1 }
				})
			};

			var result = await collection.Aggregate<GeoSearchQueryResult>(pipeline).FirstOrDefaultAsync();

			response.Success(result);
		} catch (Exception ex) {
			response.Error(ex);
		}

		return response;
	}

	public async Task<List<LocationDbEntity>?> GetLocationsWithinRadiusAsync(double latitude, double longitude, double radiusInMeters)
	{
		try {
			var query = $@"
			{{
				""location"": {{
					""$near"": {{
						""$geometry"": {{
							""type"": ""Point"",
							""coordinates"": [{longitude}, {latitude}]
						}},
						""$maxDistance"": {radiusInMeters}
					}}
				}}
			}}";

			var filter = new BsonDocumentFilterDefinition<LocationDbEntity>(BsonDocument.Parse(query));
			
			return await _locationsRepository.FindAsync(filter);
		} catch(Exception ex) {
			_logger.LogError($"|| ** Following error occured in GetLocationsWithinRadiusAsync with message: {ex.Message}");
		}

		return null;
	}

	public async Task<List<LocationDbEntity>> GetLocationsWithCityDataAsync(string cityData)
	{
		var coordinates = await _geocodingServices.GeocodeAsync(cityData);
		var response = await GetLocationsWithinRadiusAsync(coordinates.Latitude, coordinates.Longitude, 30000);

		return response ?? [];
	}

	public async Task<DSMEnvelop<GeoSearchQueryResult, LocationFacade>> GetLocationsWithCityDataV3Async(string cityData)
	{
		var coordinates = await _geocodingServices.GeocodeAsync(cityData);
		var response = await GetLocationsWithInRadiusV3Async(coordinates.Latitude, coordinates.Longitude, 30000);

		return response;
	}


	public async Task<DSMEnvelop<List<LocationDbEntity>,LocationFacade>> GetLocationsWithCityDataAsyncV2(string cityData)
	{
		var response = DSMEnvelop<List<LocationDbEntity>,LocationFacade>.Initialize(_logger);

		try {
			var coordinates = await _geocodingServices.GeocodeAsync(cityData);
			var responsData = await GetLocationsWithinRadiusAsync(coordinates.Latitude, coordinates.Longitude, 30000);
			
			// return responseData ?? [];

		} catch (Exception ex) {
			response.Error(ex);
		}

		return response;
	}

	public async Task<DSMEnvelop<GeoSearchQueryResult, LocationFacade>> GetLocationsWithZipCodesV3Async(string zipCodes)
	{
		var response = DSMEnvelop<GeoSearchQueryResult, LocationFacade>.Initialize(_logger);

		try {
			// Parse the comma-delimited string into an array of zip codes
			var zipCodeArray = zipCodes.Split(',').Select(z => z.Trim()).ToArray();

			// Construct the MongoDB query
			var pipeline = new[]
			{
				new BsonDocument("$match", new BsonDocument("geoZipCode", new BsonDocument("$in", new BsonArray(zipCodeArray)))),
				new BsonDocument("$group", new BsonDocument
				{
					{ "_id", new BsonDocument { { "longitude", "$longitude" }, { "latitude", "$latitude" } } },
					{ "count", new BsonDocument("$sum", 1) },
					{ "documents", new BsonDocument("$push", "$$ROOT") }
				}),
				new BsonDocument("$project", new BsonDocument
				{
					{ "_id", 0 },
					{ "longitude", "$_id.longitude" },
					{ "latitude", "$_id.latitude" },
					{ "count", 1 },
					{ "documents", 1 },
					{ "isDuplicate", new BsonDocument("$gt", new BsonArray { "$count", 1 }) }
				}),
				new BsonDocument("$group", new BsonDocument
				{
					{ "_id", BsonNull.Value },
					{ "groupedLocations", new BsonDocument("$push", new BsonDocument
						{
							{ "$cond", new BsonArray
								{
									new BsonDocument("$eq", new BsonArray { "$isDuplicate", true }),
									new BsonDocument
									{
										{ "longitude", "$longitude" },
										{ "latitude", "$latitude" },
										{ "count", "$count" },
										{ "documents", "$documents" }
									},
									BsonNull.Value
								}
							}
						}
					)},
					{ "singleLocations", new BsonDocument("$push", new BsonDocument
						{
							{ "$cond", new BsonArray
								{
									new BsonDocument("$eq", new BsonArray { "$isDuplicate", false }),
									new BsonDocument
									{
										{ "longitude", "$longitude" },
										{ "latitude", "$latitude" },
										{ "documents", new BsonDocument("$arrayElemAt", new BsonArray { "$documents", 0 }) }
									},
									BsonNull.Value
								}
							}
						}
					)}
				}),
				new BsonDocument("$project", new BsonDocument
				{
					{ "_id", 0 },
					{ "groupedLocations", 1 },
					{ "singleLocations", new BsonDocument("$map", new BsonDocument
						{
							{ "input", "$singleLocations" },
							{ "as", "single" },
							{ "in", new BsonDocument
								{
									{ "businessName", "$$single.documents.businessName" },
									{ "longitude", "$$single.longitude" },
									{ "latitude", "$$single.latitude" },
									{ "businessCategory", "$$single.documents.businessCategory" },
									{ "businessPhone", "$$single.documents.businessPhone" },
									{ "businessWebAddress", "$$single.documents.businessWebAddress" },
									{ "businessDescription", "$$single.documents.businessDescription" },
									{ "businessAddress", "$$single.documents.businessAddress" }
								}
							}
						}
					)}
				})
			};

			var collection = _locationsRepository.LocationsCollection;
			var result = await collection.Aggregate<GeoSearchQueryResult>(pipeline).FirstOrDefaultAsync();

			if (result == null) return response.Error(DSMEnvelopeCodeEnum.API_FACADE_04010, $"These zipcode \"{zipCodes}\" failed to return any locations.");

			response.Success(result);

		} catch (Exception ex) {
			response.Error(ex);
		}

		return response;
	}

	public async Task<List<LocationDbEntity>> GetLocationsWithZipCodesAsync(string zipCodes)
	{
		var zipCodeList = zipCodes.Split(',').Select(z => z.Trim()).ToList();
		var filter = Builders<LocationDbEntity>.Filter.In(l => l.GeoZipCode, zipCodeList);

		return await _locationsRepository.FindAsync(filter);
	}

	public async Task<DSMEnvelop<GeoCodeResponse, LocationFacade>> GetLocationsWithCityDataV2Async(string cityData)
	{
		var response = DSMEnvelop<GeoCodeResponse, LocationFacade>.Initialize(_logger);

		try {
			var coordinates = await GetCenterPointFromCityName(cityData);
			if (coordinates == null) return response.Error(DSMEnvelopeCodeEnum.API_FACADE_04010, $"Can't find coordinates for \"{cityData}\"");
			if (coordinates.Code != DSMEnvelopeCodeEnum.GEN_COMMON_00000) return response.Rebase(coordinates);
			if (coordinates.Payload == null) return response.Error(DSMEnvelopeCodeEnum.API_FACADE_04010, $"Missing payload for \"{cityData}\"");

			var responsePayload = await GetLocationsWithinRadiusAsync(coordinates.Payload.Latitude, coordinates.Payload.Longitude, 30000);
			if (responsePayload == null) return response.Error(DSMEnvelopeCodeEnum.API_FACADE_04010, $"Null resposne from \"GetLocationsWithinRadiusAsync\".");

			var geoCodeResponse = new GeoCodeResponse
			{
				TotalCount = responsePayload.Count,
				Payload = responsePayload,
				Longitude = coordinates.Payload.Longitude,
				Latitude = coordinates.Payload.Latitude
			};

			response.Success(geoCodeResponse);

		} catch(Exception ex) {
			response.Error(ex);
		}

		return response;
	}

	public async Task<DSMEnvelop<GeoCodeResponse, LocationFacade>> GetLocationsWithZipCodesV2Async(string zipCodes)
	{
		var response = DSMEnvelop<GeoCodeResponse, LocationFacade>.Initialize(_logger);

		try {
			var locationPayload = await GetLocationsWithZipCodesAsync(zipCodes);

			var zipCodeList = zipCodes.Split(',').Select(z => z.Trim()).ToList();
			var responsePayload = await GetCenterPointFromZipCodeList(zipCodeList);
			if (responsePayload == null) return response.Error(DSMEnvelopeCodeEnum.API_FACADE_04010, "When calling GetCenterPointFromZipCodeList a null is returned.");
			if (responsePayload.Code != DSMEnvelopeCodeEnum.GEN_COMMON_00000) return response.Rebase(responsePayload);
			if (responsePayload.Payload == null) return response.Error(DSMEnvelopeCodeEnum.API_FACADE_04010, "When calling GetCenterPointFromZipCodeList we get a successful code but payload is null");

			responsePayload.Payload.Payload = locationPayload;
			responsePayload.Payload.TotalCount = locationPayload.Count;

			response.Success(responsePayload.Payload);

		} catch (Exception ex) {
			response.Error(ex);
		}

		return response;
	}
 
	public async Task<List<LocationWithCoordinates>> GeoCodeAllLocations()
	{
		var locations = await _locationsRepository.GetAllAsync();
		var locationsWithCoordinates = new List<LocationWithCoordinates>();
		var ranGC = 0;
		var notRanGC = 0;
		var notSet = 0;

		foreach (var location in locations)
		{
			if (string.IsNullOrEmpty(location.BusinessAddress) || DontRunGeoCoding(location)) {
				notRanGC++;
				continue;
			}
			ranGC++;
			var coordinates = await _geocodingServices.GeocodeAsync(location.BusinessAddress);
			locationsWithCoordinates.Add(new LocationWithCoordinates
			{
				LocationID = location._id,
				Address = location.BusinessAddress ?? "[No Address Present]",
				Latitude = coordinates.Latitude,
				Longitude = coordinates.Longitude,
				Status = coordinates.Status ?? "[Not Set]"
			});

			if (string.IsNullOrEmpty(coordinates.Status)) notSet++;

			location.GeoStatus = coordinates.Status ?? "[Not Set]";
			location.Latitude = coordinates.Latitude;
			location.Longitude = coordinates.Longitude;
			location.GeoCity = coordinates.City;
			location.GeoZipCode = coordinates.PostalCode;
			location.GeoStateProv = coordinates.StateProv;
			location.GeoCounty = coordinates.County;
			location.GeoCountry = coordinates.Country;

			await _locationsRepository.UpdateAsync(location._id, location);
		}

		_logger.LogInformation($"|| ** Not Ran: {notRanGC}");
		_logger.LogInformation($"|| ** Ran: {ranGC}");
		_logger.LogInformation($"|| ** Not Set: {notSet}");

		return locationsWithCoordinates;
	}

	private bool DontRunGeoCoding(LocationDbEntity location)
	{
		if (string.IsNullOrEmpty(location.GeoStatus)
			|| !location.GeoStatus.Equals("Update")) return false;

		return true;
	}

	private async Task<DSMEnvelop<GeoCodeResponse, LocationFacade>> GetCenterPointFromZipCodeList(IList<string> zipCodeList) {
		var response = DSMEnvelop<GeoCodeResponse, LocationFacade>.Initialize(_logger);

		try {
			var geoCodeZipList = new List<GBGeoCodes>();
			foreach(var zipCode in zipCodeList) {
				var geoZipCodes = await _geocodingServices.GeocodeAsync(zipCode);
				geoCodeZipList.Add(geoZipCodes);
			}

			var geoCodeResponse = new GeoCodeResponse();
			geoCodeResponse.GenerateCenterPoint(geoCodeZipList);

			response.Success(geoCodeResponse);
		} catch(Exception ex) {
			response.Error(ex);
		}

		return response;
	}

	private async Task<DSMEnvelop<GeoCodeResponse, LocationFacade>> GetCenterPointFromCityName(string cityName) {
		var response = DSMEnvelop<GeoCodeResponse, LocationFacade>.Initialize(_logger);

		try {

			var cityGeoCode = await _geocodingServices.GeocodeAsync(cityName);

			var geoCodeResponse = new GeoCodeResponse();
			geoCodeResponse.GenerateCenterPoint(new List<GBGeoCodes> { cityGeoCode });

			response.Success(geoCodeResponse);
		} catch (Exception ex) {
			response.Error(ex);
		}

		return response;
	}

	public async Task<bool> DumbExcell() {
		try{
			var excelData = await CreateExcell();

			File.WriteAllBytes("Locations.xlsx", excelData);

			return true;
		}
		catch(Exception ex){
			_logger.LogError($"|| ** Error Dumping to Excell: {ex.Message}");
		}

		return false;
	}

	private async Task<byte[]> CreateExcell() {
		try {
			var locations = await _locationsRepository.GetAllAsync();

	        using (var workbook = new XLWorkbook())
	        {
	            var worksheet = workbook.Worksheets.Add("Locations");

	            // Add headers
				var ndx = 0;
	            worksheet.Cell(1, ++ndx).Value = "Id";
	            worksheet.Cell(1, ++ndx).Value = "BusinessName";
				worksheet.Cell(1, ++ndx).Value = "BusinessDescription";
				worksheet.Cell(1, ++ndx).Value = "BusinessWebAddress";
				worksheet.Cell(1, ++ndx).Value = "BusinessPhone";
				worksheet.Cell(1, ++ndx).Value = "BusinessAddress";
				worksheet.Cell(1, ++ndx).Value = "BusinessLogoFileUrl";
				worksheet.Cell(1, ++ndx).Value = "SubmitedOn";
				worksheet.Cell(1, ++ndx).Value = "YourName";
				worksheet.Cell(1, ++ndx).Value = "YourPositionTitle";
				worksheet.Cell(1, ++ndx).Value = "YourPhone";
				worksheet.Cell(1, ++ndx).Value = "YourEmail";
				worksheet.Cell(1, ++ndx).Value = "ShowOnMap";
				worksheet.Cell(1, ++ndx).Value = "HowDidYouHearAboutUs";
				worksheet.Cell(1, ++ndx).Value = "Source";
				worksheet.Cell(1, ++ndx).Value = "Latitude";
				worksheet.Cell(1, ++ndx).Value = "Longitude";
				worksheet.Cell(1, ++ndx).Value = "GeoStatus";
				worksheet.Cell(1, ++ndx).Value = "GeoMessage";

	            // Add data
	            for (int i = 0; i < locations.Count; i++)
	            {
					ndx = 0;
	                var location = locations[i];
	                worksheet.Cell(i + 2, ++ndx).Value = location._id;
					worksheet.Cell(i + 2, ++ndx).Value = location.BusinessName;
					worksheet.Cell(i + 2, ++ndx).Value = location.BusinessDescription;
					worksheet.Cell(i + 2, ++ndx).Value = location.BusinessWebAddress;
					worksheet.Cell(i + 2, ++ndx).Value = location.BusinessPhone;
					worksheet.Cell(i + 2, ++ndx).Value = location.BusinessAddress;
					worksheet.Cell(i + 2, ++ndx).Value = location.BusinessLogoFileUrl;
					worksheet.Cell(i + 2, ++ndx).Value = location.SubmitedOn;
					worksheet.Cell(i + 2, ++ndx).Value = location.YourName;
					worksheet.Cell(i + 2, ++ndx).Value = location.YourPositionTitle;
					worksheet.Cell(i + 2, ++ndx).Value = location.YourPhone;
					worksheet.Cell(i + 2, ++ndx).Value = location.YourEmail;
					worksheet.Cell(i + 2, ++ndx).Value = location.ShowOnMap ?? false;
					worksheet.Cell(i + 2, ++ndx).Value = location.HowDidYouHearAboutUs;
					worksheet.Cell(i + 2, ++ndx).Value = location.Source;
					worksheet.Cell(i + 2, ++ndx).Value = location.Latitude;
					worksheet.Cell(i + 2, ++ndx).Value = location.Longitude;
					worksheet.Cell(i + 2, ++ndx).Value = location.GeoStatus;
					worksheet.Cell(i + 2, ++ndx).Value = location.GeoMessage;
	            }

	            using (var stream = new MemoryStream())
	            {
	                workbook.SaveAs(stream);
	                return stream.ToArray();
	            }
	        }
		}
		catch(Exception ex) {
			_logger.LogError($"|| Error Creating to Excell: {ex.Message}");
		}

		return [];
	}
}
