namespace GBCalculatorRatesAPI.Services;

using GBCalculatorRatesAPI.Models;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using QUAD.DSM;

public partial class GoogleServices
{
	public async Task<DSMEnvelop<IList<TransactionDbEntity>,GoogleServices>> UpdateTransactionGoogleSheet(IList<TransactionDbEntity> dataList)
	{
		var response = DSMEnvelop<IList<TransactionDbEntity>,GoogleServices>.Initialize(_logger);

		try
		{
			var values = getTransactionList(dataList);
			if (values.Count == 0) return response.Warning(DSMEnvelopeCodeEnum.API_SERVICES_06001, "There are no transactions to save to google sheets");

			var info = getTransactionSheetInfo(values.Count);

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

	private IList<IList<object>> getTransactionList(IList<TransactionDbEntity> dataList)
	{
		var response = new List<IList<object>>();

		foreach(var trans in dataList)
		{
			response.Add(new List<object> {
				trans.Id,
				trans.PayloadVersion,
				trans.DeviceId,
				trans.TransactionDate,
				trans.LastUpdateDate,
				trans.Description,
				trans.Payload.Rate,
				trans.Payload.SourceCurrency,
				trans.Payload.TargetCurrency,
				trans.Payload.Price,
				trans.Payload.Due,
				trans.Payload.Payment,
				trans.Payload.Change,
				ConvertDoubles(trans.Coord?.Longitude),
				ConvertDoubles(trans.Coord?.Latitude)
			});
		}

		return response;
	}

	private string ConvertDoubles(double? value) {
		if (value == null) return string.Empty;

		return value?.ToString() ?? string.Empty;
	}

	private struct GoogleSheetInfo
	{
		public string Range { get; set; }

		public string SpreadSheetId { get; set; }
	}

	private GoogleSheetInfo getTransactionSheetInfo(int rowCount)
	{
		var range = _configuration["GSTRANSACTION_RANGE"] ?? "record_transaction!A2:O";
		var spreadSheetId = _configuration["GSTRANSACTION_SPREADSHEET_ID"] ?? "19AmWPu0mbxuiCPSW8r3G4gk6rClgbnQceIAEkr-3ik4";

		var response = new GoogleSheetInfo
		{
			Range = $"{range}{rowCount+1}",
			SpreadSheetId = spreadSheetId
		};

		return response;
	}
}