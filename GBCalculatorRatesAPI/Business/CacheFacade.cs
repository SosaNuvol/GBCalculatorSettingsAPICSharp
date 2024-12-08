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

    public CacheFacade(ILogger<CacheFacade<T>> logger, CacheRepository<T> cacheRepository, ExchangeServices exchangeServices)
    {
        _logger = logger;
        _cacheRepository = cacheRepository;
		_exchangeServices = exchangeServices;
    }

	public async Task<DSMEnvelop<T, CacheFacade<T>>> GetCacheItem() {
		var response = DSMEnvelop<T, CacheFacade<T>>.Initialize(_logger);

		try {
			var responseData = await _cacheRepository.GetLatestItem();
			if (responseData.Code != DSMEnvelopeCodeEnum.GEN_COMMON_00000
				&& responseData.Code != DSMEnvelopeCodeEnum.API_REPOS_05010) return response.Rebase(responseData);

			var payloadResult = await GetLatestPayload(responseData);

			if (payloadResult == null || payloadResult.Code != DSMEnvelopeCodeEnum.GEN_COMMON_00000) return response.Error(DSMEnvelopeCodeEnum.API_FACADE_04010, $"|| ** When in facade there was no payload for \"{Utilities.Tools.GetCacheType<T>()}\"");

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
			if(responseData.Payload.ExpiresAt >= DateTimeOffset.Now) return response.Success(responseData.Payload);

			var exchangeResponse = await _exchangeServices.GetExchangeRates();
			if (exchangeResponse.Code != DSMEnvelopeCodeEnum.GEN_COMMON_00000) return response.Rebase(exchangeResponse);

			var exchangeDbResponse = await _cacheRepository.UpdateCache((T)(object)exchangeResponse.Payload);
			if (exchangeDbResponse.Code != DSMEnvelopeCodeEnum.GEN_COMMON_00000) return response.Rebase(exchangeDbResponse);

			response.Success(exchangeDbResponse.Payload);

		} catch (Exception ex) {
			response.Error(ex, DSMEnvelopeCodeEnum.API_FACADE_04010, $"|| ** Exception in GetLatestPayload for \"{Utilities.Tools.GetCacheType<T>()}\" type.");
		}

		return response;
	}
}