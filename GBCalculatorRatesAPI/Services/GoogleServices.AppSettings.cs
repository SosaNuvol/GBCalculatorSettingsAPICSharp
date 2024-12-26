namespace GBCalculatorRatesAPI.Services;

using System.Drawing.Text;
using DocumentFormat.OpenXml.Office2013.PowerPoint.Roaming;
using GBCalculatorRatesAPI.Models;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using MongoDB.Bson.IO;
using QUAD.DSM;

public partial class GoogleServices
{
	public async Task<DSMEnvelop<AppSettingsModel,GoogleServices>> GetAppSettingsFromGoogleSheet()
	{
		var response = DSMEnvelop<AppSettingsModel,GoogleServices>.Initialize(_logger);

		try
		{
			var sheetInfo = getAppSettingsSheetInfo();
			var request = _services.Spreadsheets.Values.Get(sheetInfo.SpreadSheetId, sheetInfo.Range);

			var updateResponse = await request.ExecuteAsync();

			if (updateResponse == null) return response.Error(DSMEnvelopeCodeEnum.API_SERVICES_06001, $"Google Sheet services returned null.  There must have been an error.");

			var responseDataPayload = buildAppSettingsDataPaylaod(updateResponse);

			response.Success(responseDataPayload);

		} catch (Exception ex) {
			response.Error(ex);
		}

		return response;
	}

	private AppSettingsModel buildAppSettingsDataPaylaod(ValueRange? valueRange)
	{
		Console.WriteLine($"Here is the value range: {valueRange}");

		var startingPoint = new GeoCircle {
			Coordinates = [1,2],
			Type = "CircleArea",
			RadiusMiles = 5
		};

		var result = new AppSettingsModel {
			DefaultSearchRadius = 2,
			PercentageChangeRate = 1,
			DefaultStartingPoint = startingPoint
		};

		if (valueRange == null) return result;

		foreach(var row in valueRange.Values)
		{
			switch(row[(int)AppSettingsColumns.Key].ToString())
			{
				case "ChangeRatePercentage":
					result.PercentageChangeRate = Convert.ToInt32(row[(int)AppSettingsColumns.Values]);
					break;
				
				case "DefaultSearchRadiusMiles":
					result.DefaultSearchRadius = Convert.ToInt32(row[(int)AppSettingsColumns.Values]);
					break;

				case "DefaultStartingPoint":
					startingPoint.RadiusMiles = Convert.ToInt32(row[(int)AppSettingsColumns.Values]);
					startingPoint.Coordinates = [Convert.ToDouble(row[(int)AppSettingsColumns.Values + 1]),Convert.ToDouble(row[(int)AppSettingsColumns.Values + 2])];
					break;
			}
		}

		return result;
	}

	private enum AppSettingsColumns
	{
		Key = 0,
		Description = 1,
		Type = 2,
		Values = 3
	}

	private GoogleSheetInfo getAppSettingsSheetInfo(int rowCount = 0)
	{
		var range = _configuration["GSAPPSETTNGS_RANGE"] ?? "app_settings!A2:F";
		var spreadSheetId = _configuration["GSAPPSETTNGS_SPREADSHEET_ID"] ?? "19AmWPu0mbxuiCPSW8r3G4gk6rClgbnQceIAEkr-3ik4";

		var response = new GoogleSheetInfo
		{
			Range = rowCount == 0 ? $"{range}" : $"{range}{rowCount+1}",
			SpreadSheetId = spreadSheetId
		};

		return response;
	}
}