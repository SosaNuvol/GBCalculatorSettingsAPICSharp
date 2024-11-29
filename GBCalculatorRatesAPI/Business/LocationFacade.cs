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

public class LocationFacade
{
	private readonly ILogger<LocationFacade> _logger;
	private readonly LocationsRepository _locationsRepository;
	private readonly GeocodingService _geocodingServices;

	public LocationFacade(ILogger<LocationFacade> logger, LocationsRepository locationsRepository, GeocodingService geocodingServices)
	{
		_logger = logger;
		_locationsRepository = locationsRepository;
		_geocodingServices = geocodingServices;
	}
 
	public async Task<List<LocationWithCoordinates>> GetLocationsWithCoordinates()
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

			await _locationsRepository.UpdateAsync(location._id, location);
		}

		_logger.LogInformation($"|| ** Not Ran: {notRanGC}");
		_logger.LogInformation($"|| ** Ran: {ranGC}");
		_logger.LogInformation($"|| ** Not Set: {notSet}");

		return locationsWithCoordinates;
	}

	private bool DontRunGeoCoding(Location location)
	{
		if (string.IsNullOrEmpty(location.GeoStatus)
			|| !location.GeoStatus.Equals("Update")) return false;

		return true;
	}

	public async Task<bool> DumbExcell() {
		try{
			var excelData = await CreateExcell();

			File.WriteAllBytes("Locations.xlsx", excelData);

			return true;
		}
		catch(Exception ex){
			_logger.LogError("|| Error Dumping to Excell: ", ex.Message);
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
