namespace GBCalculatorRatesAPI.Business;

using System.Configuration;
using GBCalculatorRatesAPI.Models;
using GBCalculatorRatesAPI.Services;
using Microsoft.Extensions.Logging;
using QUAD.DSM;

public class SettingsFacade
{
	private readonly ILogger<SettingsFacade> _logger;

	private readonly CacheFacade<AppSettingsModel> _cacheAppSettingsFacade;
	
	private readonly GoogleServices _googleServices;

	public SettingsFacade(CacheFacade<AppSettingsModel> cacheAppSettingsFacade, GoogleServices googleServices, ILogger<SettingsFacade> logger)
	{
		_logger = logger;
		_googleServices = googleServices;
		_cacheAppSettingsFacade = cacheAppSettingsFacade;
	}

	public async Task<DSMEnvelop<AppSettingsModel, SettingsFacade>> GetAppSettings()
	{
		var response = DSMEnvelop<AppSettingsModel, SettingsFacade>.Initialize(_logger);

		try
		{
			var cachedAppSettingsResponse = await _cacheAppSettingsFacade.GetCacheItem();

			if (cachedAppSettingsResponse.Code == DSMEnvelopeCodeEnum._SUCCESS) return response.Success(cachedAppSettingsResponse.Payload);
			
			if (cachedAppSettingsResponse.Code != DSMEnvelopeCodeEnum._SUCCESS
				&& cachedAppSettingsResponse.Code != DSMEnvelopeCodeEnum.API_REPOS_05010) return response.Rebase(cachedAppSettingsResponse);

			if (cachedAppSettingsResponse.Code == DSMEnvelopeCodeEnum.API_REPOS_05010) {
				var appSettingsResponse = await _googleServices.GetAppSettingsFromGoogleSheet();
				if (appSettingsResponse.Code != DSMEnvelopeCodeEnum._SUCCESS) return response.Rebase(appSettingsResponse);

				var updateCacheResponse = await _cacheAppSettingsFacade.UpdateCacheItem(appSettingsResponse.Payload);
				if (updateCacheResponse.Code != DSMEnvelopeCodeEnum._SUCCESS) return response.Rebase(updateCacheResponse);
				
				response.Success(appSettingsResponse.Payload);
			}
			
			response.Rebase(cachedAppSettingsResponse);

		} catch(Exception ex) {
			response.Error(ex);
		}

		return response;
	}
		
}