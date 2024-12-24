namespace GBCalculatorRatesAPI.Services;

using GBCalculatorRatesAPI.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QUAD.DSM;

public partial class GoogleServices
{
	private const string _APPLICATION_NAME = "Goldback Calculator Mobile Application API v1";
	private readonly string _SPREADSHEETID;
	private readonly string _RANGE_DIST_LIST;
	private readonly string _RANGE_MRCH_LIST;

	private readonly string _apiKey;

	private readonly ILogger<GoogleServices> _logger;

	private readonly IConfiguration _configuration;

	private readonly SheetsService _services;

	private readonly SheetsService _sheetServiceWithServiceAccount;

	public GoogleServices(string apiKey, ILogger<GoogleServices> logger, IConfiguration configuration)
	{
		_apiKey = apiKey;
		_logger = logger;
		_configuration = configuration;

		_services =  new SheetsService(new BaseClientService.Initializer()
        {
            ApiKey = apiKey,
            ApplicationName = _APPLICATION_NAME,
        });

		_RANGE_DIST_LIST = _configuration["GSDISTRIBU_RANGE"] ?? "Distributors!A2:F";
		_RANGE_MRCH_LIST = _configuration["GSLOCATION_RANGE"] ?? "A2:O";
		
		_SPREADSHEETID = _configuration["GSLOCATION_SPREADSHEET_ID"] ?? "1uc8BW6_hHW2E3Za5ZQEHv1rpnCQmNnzCS6MPNmtA9p0";

		_sheetServiceWithServiceAccount = InitSheetService();
	}

	private SheetsService InitSheetService()
	{
		string[] Scopes = { SheetsService.Scope.Spreadsheets };
        string ApplicationName = "Google Sheets API .NET Quickstart";

        // Authenticate using the service account credentials
        var credential = GoogleCredential.FromFile("./goldbackmobileapp-34d8ce2a8264.json")
            .CreateScoped(Scopes);

        // Create the Sheets API service
        var service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });

		return service;
	}

	public async Task<DSMEnvelop<LocationResponseModel,GoogleServices>> getLocations()
	{
		var response = DSMEnvelop<LocationResponseModel,GoogleServices>.Initialize(_logger);

		try
		{
			var tabsResponse = await getAllTabs(_SPREADSHEETID);
			if (tabsResponse.Code != DSMEnvelopeCodeEnum._SUCCESS) return response.Rebase(tabsResponse);

			var merchSheetNameList = tabsResponse.Payload
				.Where(name => !name.Equals(LocationConstants.DISTRIBUTOR_LIST_NAME, StringComparison.OrdinalIgnoreCase))
				.ToList();

			var idCounter = 0;
			var distListResponse = await getAllDistributors(idCounter, _SPREADSHEETID, _RANGE_DIST_LIST);
			if (distListResponse.Code != DSMEnvelopeCodeEnum._SUCCESS) return response.Rebase(distListResponse);

			var merchList = new List<ILocation>();
			foreach(var tabName in merchSheetNameList)
			{
				var payloadListResponse = await getAllMerchants(idCounter, _SPREADSHEETID, $"{tabName}!{_RANGE_MRCH_LIST}", distListResponse.Payload, merchList);
				if (payloadListResponse.Code != DSMEnvelopeCodeEnum._SUCCESS) return response.Rebase(payloadListResponse);
			}

			// Merge the two lists
			var mergedList = distListResponse.Payload.Concat(merchList).ToList();

			response.Success(new LocationResponseModel(mergedList));
		} catch(Exception ex) {
			response.Error(ex);
		}

		return response;
	}

	private async Task<DSMEnvelop<IList<ILocation>,GoogleServices>> getAllMerchants(int idCounter, string spreadSheetId, string range, IList<ILocation> distList, IList<ILocation> merchList)
	{
		var response = DSMEnvelop<IList<ILocation>,GoogleServices>.Initialize(_logger);

		try
		{
			var listPayload = new List<ILocation>();
			var request = _services.Spreadsheets.Values.Get(spreadSheetId, range);
			var merchListResponse = await request.ExecuteAsync();

			if (merchListResponse == null || merchListResponse.Values == null) return response.Success(listPayload);

			foreach(var row in merchListResponse.Values)
			{
				if (findDistTry(row, distList, out var foundItem) && foundItem != null)
				{
					foundItem.BusinessCategory = LocationConstants.BOTH;
					foundItem.BusinessDescription = getCellValue(row, (int)LocationColumnIndex.DescriptionOfBusiness);
					foundItem.BusinessWebAddress = getCellValue(row, (int)LocationColumnIndex.BusinessWebAddress);
					foundItem.BusinessPhone = getCellValue(row, (int)LocationColumnIndex.BusinessPhone);
					foundItem.BusinessAddress = getCellValue(row, (int)LocationColumnIndex.BusinessAddress);
					foundItem.BusinessLogoFileUrl = getCellValue(row, (int)LocationColumnIndex.BusinessLogoImageFile);
					foundItem.SubmitedOn = getCellValue(row, (int)LocationColumnIndex.SubmittedOn);
					foundItem.YourName = getCellValue(row, (int)LocationColumnIndex.YourName);
					foundItem.YourPositionTitle = getCellValue(row, (int)LocationColumnIndex.YourPositionTitle);
					foundItem.YourPhone = getCellValue(row, (int)LocationColumnIndex.YourPhone);
					foundItem.YourEmail = getCellValue(row, (int)LocationColumnIndex.YourEmail);
					foundItem.ShowOnMap = bool.TryParse(getCellValue(row, (int)LocationColumnIndex.IncludeBusiness), out var showMap) ? showMap : false;
					foundItem.HowDidYouHearAboutUs = getCellValue(row, (int)LocationColumnIndex.HowDidYouHearAboutUs);

				} else {
					idCounter++;
					var location = new LocationDbEntity() {
						id = idCounter,
						BusinessName = getCellValue(row, (int)LocationColumnIndex.BusinessName),
						BusinessCategory = LocationConstants.MERCHANT,
						BusinessDescription = getCellValue(row, (int)LocationColumnIndex.DescriptionOfBusiness),
						BusinessWebAddress = getCellValue(row, (int)LocationColumnIndex.BusinessWebAddress),
						BusinessPhone = getCellValue(row, (int)LocationColumnIndex.BusinessPhone),
						BusinessAddress = getCellValue(row, (int)LocationColumnIndex.BusinessAddress),
						BusinessLogoFileUrl = getCellValue(row, (int)LocationColumnIndex.BusinessLogoImageFile),
						SubmitedOn = getCellValue(row, (int)LocationColumnIndex.SubmittedOn),
						YourName = getCellValue(row, (int)LocationColumnIndex.YourName),
						YourPositionTitle = getCellValue(row, (int)LocationColumnIndex.YourPositionTitle),
						YourPhone = getCellValue(row, (int)LocationColumnIndex.YourPhone),
						YourEmail = getCellValue(row, (int)LocationColumnIndex.YourEmail),
						ShowOnMap = bool.TryParse(getCellValue(row, (int)LocationColumnIndex.IncludeBusiness), out var showMap) ? showMap : false,
						HowDidYouHearAboutUs = getCellValue(row, (int)LocationColumnIndex.HowDidYouHearAboutUs),
						Source = range ?? "[Not Set]"
					};

					merchList.Add(location);
				}

				response.Success(merchList);
			}

		} catch(Exception ex) {
			response.Error(ex);
		}

		return response;
	}

	private bool findDistTry(IList<object> row, IList<ILocation> distList, out ILocation? foundItem) {
		foundItem = null;
		var businessName = row[(int)LocationColumnIndex.BusinessName]?.ToString();

		if (string.IsNullOrEmpty(businessName)) return false;

		businessName = businessName.Trim().ToUpper();

		foundItem = distList.FirstOrDefault(dist => !string.IsNullOrEmpty(dist.BusinessName) && dist.BusinessName.Trim().ToUpper().Equals(businessName));

		if (foundItem == null) return false;

		return true;
	}

	private async Task<DSMEnvelop<IList<string>,GoogleServices>> getAllTabs(string spreadSheetId)
	{
		var response = DSMEnvelop<IList<string>,GoogleServices>.Initialize(_logger);

		try
		{
			var request = _services.Spreadsheets.Get(spreadSheetId);
			var spreadsheet = await request.ExecuteAsync();

			var tabs = spreadsheet.Sheets.Select(sheet => sheet.Properties.Title).ToList();

			response.Success(tabs);
		} catch(Exception ex) {
			response.Error(ex);
		}

		return response;
	}

	private async Task<DSMEnvelop<IList<ILocation>,GoogleServices>> getAllDistributors(int idCounter, string spreadSheetId, string range)
	{
		var response = DSMEnvelop<IList<ILocation>,GoogleServices>.Initialize(_logger);

		try
		{
			var listPayload = new List<ILocation>();
			var request = _services.Spreadsheets.Values.Get(spreadSheetId, range);
			var distList = await request.ExecuteAsync();

			foreach(var row in distList.Values)
			{
				idCounter++;
				var distLocation = new LocationDbEntity();
				distLocation.id = idCounter;
				distLocation.BusinessName = getCellValue(row, (int)DistributorColumnIndex.BusinessCompanyName);
				distLocation.BusinessCategory = LocationConstants.DISTRIBUTOR;
				distLocation.BusinessDescription = string.Empty;
				distLocation.BusinessWebAddress = getCellValue(row, (int)DistributorColumnIndex.BusinessWebAddress);
				distLocation.BusinessPhone = getCellValue(row, (int)DistributorColumnIndex.BusinessPhone);
				distLocation.BusinessAddress = getBusinessAddress(row);
				distLocation.BusinessLogoFileUrl = string.Empty;
				distLocation.SubmitedOn = DateTimeOffset.Now.ToString("o");  // ToISOString()
				distLocation.YourName = string.Empty;
				distLocation.YourPositionTitle = string.Empty;
				distLocation.YourPhone = string.Empty;
				distLocation.YourEmail = string.Empty;
				distLocation.ShowOnMap = true;
				distLocation.HowDidYouHearAboutUs = string.Empty;
				distLocation.Source = LocationConstants.DISTRIBUTOR;			

				listPayload.Add(distLocation);
			}

			response.Success(listPayload);

		} catch(Exception ex) {
			Console.WriteLine($"|| ** Error at index: \"{idCounter}\".");
			response.Error(ex);
		}

		return response;
	}

	private string? getCellValue(IList<object> row, int index) {
		if (row.Count <= index) return null;

		var result = row[index].ToString();

		return result?.Trim();
	}

	private string? getBusinessAddress(IList<object>row)
	{
		return (row.Count > (int)DistributorColumnIndex.BusinessBillingAddress && row[(int)DistributorColumnIndex.BusinessBillingAddress] != null)
							? row[(int)DistributorColumnIndex.BusinessBillingAddress].ToString()
							: (row.Count > (int)DistributorColumnIndex.ShippingAddress && row[(int)DistributorColumnIndex.ShippingAddress] != null)
								? row[(int)DistributorColumnIndex.ShippingAddress].ToString()
								: string.Empty;
	}


	public async Task<DSMEnvelop<LocationDbEntity, GoogleServices>> UpdateLocation(LocationDbEntity location)
	{
		var response = DSMEnvelop<LocationDbEntity, GoogleServices>.Initialize(_logger);

		try
		{
			
		} catch (Exception ex) {
			response.Error(ex);
		}

		return response;
	}

}