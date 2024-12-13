namespace GBCalculatorRatesAPI.Business;

using GBCalculatorRatesAPI.Models;
using GBCalculatorRatesAPI.Repos;
using GBCalculatorRatesAPI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QUAD.DSM;

public class RateChangeFacade 
{
	private readonly ILogger<RateChangeFacade> _logger;
	private readonly RateChangeRepository _rateChangeRepository;
	private readonly CacheFacade<ExchangeRateModel> _cacheFacade;
	private readonly ExchangeServices _exchangeServices;
	private readonly IConfiguration _configuration;

	public RateChangeFacade(ILogger<RateChangeFacade> logger, RateChangeRepository rateChangeRepository, CacheFacade<ExchangeRateModel> cacheFacade, ExchangeServices exchangeServices, IConfiguration configuration)
	{
		_logger = logger;
		_rateChangeRepository = rateChangeRepository;
		_cacheFacade = cacheFacade;
		_exchangeServices = exchangeServices;
		_configuration = configuration;
	}

	public async Task<DSMEnvelop<ExchangeRateModel, RateChangeFacade>> SaveChange(RateChangeRequest req) {
		var response = DSMEnvelop<ExchangeRateModel, RateChangeFacade>.Initialize(_logger);
		var newRate = req.CurrentRate;

		try {
			var rateChangeEntity = new RateChangeDbEntity {
				DeviceId = req.DeviceId,
				PreviousRate = req.PreviousRate,
				CurrentRate = req.CurrentRate,
				TimeStamp = DateTimeOffset.Now.ToString(),
				LocationPoint = req.GeoLocation
			};

			var rateChangeDbEntity = await _rateChangeRepository.CreateAsync(rateChangeEntity);
			if (rateChangeDbEntity.Code != DSMEnvelopeCodeEnum.GEN_COMMON_00000) response.Rebase(rateChangeDbEntity);

			// TODO.  Refactor this to get latest Exchange Rates.
			// var exchangeRates = await _exchangeServices.GetExchangeRates();
			// if (exchangeRates.Code != DSMEnvelopeCodeEnum.GEN_COMMON_00000) return response.Rebase(exchangeRates);
			// if (exchangeRates.Payload == null) return response.Error(DSMEnvelopeCodeEnum.API_FACADE_04010, "No values were return from th exchange.");
			var exchangeRatesResponse = await _cacheFacade.GetCacheItem();
			if (exchangeRatesResponse == null) return response.Error(DSMEnvelopeCodeEnum.API_FACADE_04010, "Response payload is null");
			if (exchangeRatesResponse.Code != DSMEnvelopeCodeEnum.GEN_COMMON_00000) return response.Rebase(exchangeRatesResponse);
			if (exchangeRatesResponse.Payload == null) return response.Error(DSMEnvelopeCodeEnum.API_FACADE_04010, "Exchange rates payload is null");

			var responsePayload = CreateResponse(newRate, exchangeRatesResponse.Payload);
			if (responsePayload == null) return response.Error(DSMEnvelopeCodeEnum.API_FACADE_04010, "Response payload is null");
			if (responsePayload.Code != DSMEnvelopeCodeEnum.GEN_COMMON_00000) return response.Rebase(responsePayload);
			if (responsePayload.Payload == null) return response.Error(DSMEnvelopeCodeEnum.API_FACADE_04010, "Response final payload is null.");

			response.Success(responsePayload.Payload);

		} catch (Exception ex) {
			response.Error(ex);
		}

		return response;
	}

	private DSMEnvelop<ExchangeRateModel, RateChangeFacade> CreateResponse(decimal rate, ExchangeRateModel exchangeRates) {
		var response = DSMEnvelop<ExchangeRateModel, RateChangeFacade>.Initialize(_logger);

		try {
			var currenciesListString = _configuration["CURRENCY_LIST"];
			if (string.IsNullOrEmpty(currenciesListString)) return response.Error(DSMEnvelopeCodeEnum.API_FACADE_04010, "Missing Currencies list");

			var acceptedListOfCurrencies = Newtonsoft.Json.JsonConvert.DeserializeObject<List<NameKeyModel>>(currenciesListString);
			if (acceptedListOfCurrencies == null) return response.Error(DSMEnvelopeCodeEnum.API_FACADE_04010, "No list of currencies is available");

			var acceptedKeys = acceptedListOfCurrencies.Select(c => c.Key).ToHashSet();
			var filteredQuotes = exchangeRates.Quotes
				.Where(q => acceptedKeys.Contains(q.Key))
				.ToDictionary(q => q.Key, q => q.Value * rate);

			filteredQuotes.Add("USDUSD", rate);

			var payload = new ExchangeRateModel
			{
				Terms = exchangeRates.Terms,
				Privacy = exchangeRates.Privacy,
				Timestamp = exchangeRates.Timestamp,
				Source = exchangeRates.Source,
				Quotes = filteredQuotes
			};

			response.Success(payload);

		} catch (Exception ex) {
			response.Error(ex);
		}

		return response;
	}

	public async Task<DSMEnvelop<List<ExchangeRateModel>, RateChangeFacade>> GetAllRateChanges(bool removeFromDb = false) {
		var response = DSMEnvelop<List<ExchangeRateModel>, RateChangeFacade>.Initialize(_logger);

		return response;
	}
}