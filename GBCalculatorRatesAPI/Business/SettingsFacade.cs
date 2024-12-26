namespace GBCalculatorRatesAPI.Business;

using System.Configuration;
using GBCalculatorRatesAPI.Models;
using GBCalculatorRatesAPI.Services;
using Microsoft.Extensions.Logging;
using QUAD.DSM;

public class SettingsFacade
{
	private readonly ILogger<SettingsFacade> _logger;

	private readonly GoogleServices _googleServices;

	public SettingsFacade(GoogleServices googleServices, ILogger<SettingsFacade> logger)
	{
		_logger = logger;
		_googleServices = googleServices;
	}

	public async Task<DSMEnvelop<AppSettingsModel, SettingsFacade>> GetAppSettings()
	{
		var response = DSMEnvelop<AppSettingsModel, SettingsFacade>.Initialize(_logger);

		try
		{
			var appSettingsResponse = await _googleServices.GetAppSettingsFromGoogleSheet();
			if (appSettingsResponse.Code != DSMEnvelopeCodeEnum._SUCCESS) return response.Rebase(appSettingsResponse);

			response.Success(appSettingsResponse.Payload);

		} catch(Exception ex) {
			response.Error(ex);
		}

		return response;
	}
		
}