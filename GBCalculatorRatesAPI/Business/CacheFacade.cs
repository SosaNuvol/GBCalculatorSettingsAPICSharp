namespace GBCalculatorRatesAPI.Business;

using GBCalculatorRatesAPI.Models;
using GBCalculatorRatesAPI.Repos;
using GBCalculatorRatesAPI.Services;
using Microsoft.Extensions.Logging;
using QUAD.DSM;

public class CacheFacade<T>
{
	private readonly ILogger<CacheFacade<T>> _logger;

	private readonly CacheRepository<T> _cacheRepository;

	private readonly ExchangeServices _exchangeServices;

	private readonly UPMAServices _upmaServices;

	private readonly GoogleServices _googleServices;

    public CacheFacade(ILogger<CacheFacade<T>> logger, CacheRepository<T> cacheRepository, ExchangeServices exchangeServices, UPMAServices upmaServices, GoogleServices googleServices)
    {
        _logger = logger;
        _cacheRepository = cacheRepository;
		_exchangeServices = exchangeServices;
		_upmaServices = upmaServices;
		_googleServices = googleServices;
    }

	public async Task<DSMEnvelop<T, CacheFacade<T>>> GetCacheItem() {
		var response = DSMEnvelop<T, CacheFacade<T>>.Initialize(_logger);

		try {
			var responseData = await _cacheRepository.GetLatestItem();
			if (responseData.Code != DSMEnvelopeCodeEnum._SUCCESS
				&& responseData.Code != DSMEnvelopeCodeEnum.API_REPOS_05010) return response.Rebase(responseData);

			var payloadResult = await GetLatestPayload(responseData);

			if (payloadResult == null || payloadResult.Code != DSMEnvelopeCodeEnum._SUCCESS) return response.Error(DSMEnvelopeCodeEnum.API_FACADE_04010, $"|| ** When in facade there was no payload for \"{Utilities.Tools.GetCacheType<T>()}\"");
			if (payloadResult.Payload == null) return response.Error(DSMEnvelopeCodeEnum.API_FACADE_04010, "The 'Payload' of payloadResult in GetCacheItem is null");

			response.Success(payloadResult.Payload.Payload);

		} catch (Exception ex) {
			response.Error(ex, DSMEnvelopeCodeEnum.API_FACADE_04010, $"|| ** Exception in GetCacheItem for \"{Utilities.Tools.GetCacheType<T>()}\" type.");
		}

		return response;
	}

	private async Task<DSMEnvelop<CacheDbEntity<T>, CacheFacade<T>>> GetLatestPayload(DSMEnvelop<CacheDbEntity<T>, CacheRepository<T>> responseData)
	{
		var response = DSMEnvelop<CacheDbEntity<T>, CacheFacade<T>>.Initialize(_logger);

		try {
			if (responseData == null) throw new Exception($"Response data can't be null. Fatal error");
			if(responseData.Payload != null && responseData.Payload.ExpiresAt >= DateTimeOffset.Now) return response.Success(responseData.Payload);

			switch(typeof(T).Name)
			{
				case "ExchangeRateModel":
					return await GetLatestExchangeRates(responseData);

				case "GBPricesModel":
					return await GetLatestGBPricesModel(responseData);

				case "AppSettingsModel":
					return await GetLatestAppSettingsModel(responseData);
				
				default:
					return response.Error(DSMEnvelopeCodeEnum.API_FACADE_04010, $"The type {typeof(T)} is not implemented yet.");
			}

		} catch (Exception ex) {
			response.Error(ex, DSMEnvelopeCodeEnum.API_FACADE_04010, $"|| ** Exception in GetLatestPayload for \"{Utilities.Tools.GetCacheType<T>()}\" type.");
		}

		return response;
	}

	private async Task<DSMEnvelop<CacheDbEntity<T>, CacheFacade<T>>> GetLatestExchangeRates(DSMEnvelop<CacheDbEntity<T>, CacheRepository<T>> responseData)
	{
		var response = DSMEnvelop<CacheDbEntity<T>, CacheFacade<T>>.Initialize(_logger);

		try
		{
			var exchangeResponse = await _exchangeServices.GetExchangeRates();
			if (exchangeResponse.Code != DSMEnvelopeCodeEnum._SUCCESS) return response.Rebase(exchangeResponse);

			if (exchangeResponse.Payload == null) return response.Error(DSMEnvelopeCodeEnum.API_FACADE_04010, "Exchange response payload is null.");
			var exchangeDbResponse = await _cacheRepository.UpdateCache((T)(object)exchangeResponse.Payload);
			if (exchangeDbResponse.Code != DSMEnvelopeCodeEnum._SUCCESS) return response.Rebase(exchangeDbResponse);

			if (exchangeDbResponse.Payload == null) return response.Error(DSMEnvelopeCodeEnum.API_FACADE_04010, "Payload is null.");

			response.Success(exchangeDbResponse.Payload);
		
		} catch (Exception ex) {
			response.Error(ex);
		}

		return response;
	}

	private async Task<DSMEnvelop<CacheDbEntity<T>, CacheFacade<T>>> GetLatestGBPricesModel(DSMEnvelop<CacheDbEntity<T>, CacheRepository<T>> responseData)
	{
		var response = DSMEnvelop<CacheDbEntity<T>, CacheFacade<T>>.Initialize(_logger);

		try
		{
			var gbPricesResponse = await _upmaServices.GetRatesAsync();
			if (gbPricesResponse.Code != DSMEnvelopeCodeEnum._SUCCESS) return response.Rebase(gbPricesResponse);

			if (gbPricesResponse.Payload == null) return response.Error(DSMEnvelopeCodeEnum.API_FACADE_04010, "Exchange response payload is null.");
			var gbPricesDbResponse = await _cacheRepository.UpdateCache((T)(object)gbPricesResponse.Payload);
			if (gbPricesDbResponse.Code != DSMEnvelopeCodeEnum._SUCCESS) return response.Rebase(gbPricesDbResponse);

			if (gbPricesDbResponse.Payload == null) return response.Error(DSMEnvelopeCodeEnum.API_FACADE_04010, "Payload is null.");

			response.Success(gbPricesDbResponse.Payload);
		
		} catch (Exception ex) {
			response.Error(ex);
		}

		return response;
	}

	private async Task<DSMEnvelop<CacheDbEntity<T>, CacheFacade<T>>> GetLatestAppSettingsModel(DSMEnvelop<CacheDbEntity<T>, CacheRepository<T>> responseData)
	{
		var response = DSMEnvelop<CacheDbEntity<T>, CacheFacade<T>>.Initialize(_logger);

		try
		{
			var appSettingsGoogleResponse = await _googleServices.GetAppSettingsFromGoogleSheet();
			if (appSettingsGoogleResponse.Code != DSMEnvelopeCodeEnum._SUCCESS) return response.Rebase(appSettingsGoogleResponse);

			var appSettingsDbResponse = await _cacheRepository.UpdateCache((T)(object)appSettingsGoogleResponse.Payload);
			if (appSettingsDbResponse.Code != DSMEnvelopeCodeEnum._SUCCESS) return response.Rebase(appSettingsDbResponse);

			response.Success(appSettingsDbResponse.Payload);
		
		} catch (Exception ex) {
			response.Error(ex);
		}

		return response;
	}

	public async Task<DSMEnvelop<CacheDbEntity<T>, CacheFacade<T>>> UpdateCacheItem(T responseData)
	{
		var response = DSMEnvelop<CacheDbEntity<T>, CacheFacade<T>>.Initialize(_logger);

		try
		{
			if (responseData == null) return response.Error(DSMEnvelopeCodeEnum.API_FACADE_04010, $"The passed responseData of T type \"{typeof(T).ToString()}\" is null.");

			var appSettingsDbResponse = await _cacheRepository.UpdateCache((T)(object)responseData);
			if (appSettingsDbResponse.Code != DSMEnvelopeCodeEnum._SUCCESS) return response.Rebase(appSettingsDbResponse);

			response.Success(appSettingsDbResponse.Payload);
		} catch (Exception ex) {
			response.Error(ex);
		}
		
		return response;
	}
}