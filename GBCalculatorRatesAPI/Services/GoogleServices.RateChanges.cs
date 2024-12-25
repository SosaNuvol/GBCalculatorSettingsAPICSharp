namespace GBCalculatorRatesAPI.Services;

using GBCalculatorRatesAPI.Models;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using QUAD.DSM;

public partial class GoogleServices
{
	public async Task<DSMEnvelop<IList<RateChangeDbEntity>,GoogleServices>> UpdateRateChangeGoogleSheet(IList<RateChangeDbEntity> dataList)
	{
		var response = DSMEnvelop<IList<RateChangeDbEntity>,GoogleServices>.Initialize(_logger);

		try
		{
			var values = getRateChangeList(dataList);
			if (values.Count == 0) return response.Warning(DSMEnvelopeCodeEnum.API_SERVICES_06001, "There are no transactions to save to google sheets");

			var info = getRateChangeSheetInfo(values.Count);

			var valueRange = new ValueRange {
				Values = values
			};

			var updateRequest =  _sheetServiceWithServiceAccount.Spreadsheets.Values.Update(valueRange, info.SpreadSheetId, info.Range);
			updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;

			var updateResponse = await updateRequest.ExecuteAsync();

			response.Success(dataList);

		} catch(Exception ex) {
			response.Error(ex);
		}

		return response;
	}

	private IList<IList<object>> getRateChangeList(IList<RateChangeDbEntity> dataList)
	{
		var response = new List<IList<object>>();

		foreach(var rateChange in dataList)
		{
			response.Add(new List<object> {
				rateChange.Id,
				rateChange.PayloadVersion,
				rateChange.DeviceId,
				rateChange.Source,
				rateChange.PreviousRate,
				rateChange.CurrentRate,
				GetCoordinate(rateChange.LocationPoint, CoordName.Longitude),
				GetCoordinate(rateChange.LocationPoint, CoordName.Latitude),
				rateChange.TimeStamp
			});
		}

		return response;
	}

	private enum CoordName
	{
		Longitude,
		Latitude
	}

	private string GetCoordinate(GeoLocationPoint? point, CoordName coordName)
	{
		if (point == null || point.Coordinates == null) return string.Empty;

		switch(coordName)
		{
			case CoordName.Longitude:
				return point.Coordinates[0].ToString();

			case CoordName.Latitude:
				return point.Coordinates[1].ToString();

			default:
				return string.Empty;
		}
	}


	private GoogleSheetInfo getRateChangeSheetInfo(int rowCount)
	{
		var range = _configuration["GSRATECHANGE_RANGE"] ?? "record_ratechanges!A2:O";
		var spreadSheetId = _configuration["GSRATECHANGE_SPREADSHEET_ID"] ?? "19AmWPu0mbxuiCPSW8r3G4gk6rClgbnQceIAEkr-3ik4";

		var response = new GoogleSheetInfo
		{
			Range = $"{range}{rowCount+1}",
			SpreadSheetId = spreadSheetId
		};

		return response;
	}

}