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
	private readonly GoogleServices _googleServices;
	private readonly CacheFacade<ExchangeRateModel> _cacheCurrencyRatesFacade;
	private readonly UPMAServices _upmaServices;
	private readonly IConfiguration _configuration;

	public RateChangeFacade(ILogger<RateChangeFacade> logger, GoogleServices googleServices, RateChangeRepository rateChangeRepository, CacheFacade<ExchangeRateModel> cacheFacade, UPMAServices upmaServices, IConfiguration configuration)
	{
		_logger = logger;
		_googleServices = googleServices;
		_rateChangeRepository = rateChangeRepository;
		_cacheCurrencyRatesFacade = cacheFacade;
		_upmaServices = upmaServices;
		_configuration = configuration;
	}

	public async Task<DSMEnvelop<ExchangeRateModel, RateChangeFacade>> SaveChange(RateChangeRequest req) {
		var response = DSMEnvelop<ExchangeRateModel, RateChangeFacade>.Initialize(_logger);
		var newRate = req.CurrentRate;
		var source = req.Source;

		try {
			var rateChangeEntity = new RateChangeDbEntity {
				DeviceId = req.DeviceId,
				PayloadVersion = "1.0",
				Source = req.Source,
				PreviousRate = req.PreviousRate,
				CurrentRate = req.CurrentRate,
				TimeStamp = DateTimeOffset.Now.ToString("o"),
				LocationPoint = req.GeoLocation
			};

			var rateChangeDbEntity = await _rateChangeRepository.CreateAsync(rateChangeEntity);
			if (rateChangeDbEntity.Code != DSMEnvelopeCodeEnum._SUCCESS) response.Rebase(rateChangeDbEntity);

			var exchangeRatesResponse = await _cacheCurrencyRatesFacade.GetCacheItem();
			if (exchangeRatesResponse == null) return response.Error(DSMEnvelopeCodeEnum.API_FACADE_04010, "Response payload is null");
			if (exchangeRatesResponse.Code != DSMEnvelopeCodeEnum._SUCCESS) return response.Rebase(exchangeRatesResponse);
			if (exchangeRatesResponse.Payload == null) return response.Error(DSMEnvelopeCodeEnum.API_FACADE_04010, "Exchange rates payload is null");

			var responsePayload = await CreateResponse(newRate, source, exchangeRatesResponse.Payload);
			if (responsePayload == null) return response.Error(DSMEnvelopeCodeEnum.API_FACADE_04010, "Response payload is null");
			if (responsePayload.Code != DSMEnvelopeCodeEnum._SUCCESS) return response.Rebase(responsePayload);
			if (responsePayload.Payload == null) return response.Error(DSMEnvelopeCodeEnum.API_FACADE_04010, "Response final payload is null.");

			response.Success(responsePayload.Payload);

		} catch (Exception ex) {
			response.Error(ex);
		}

		return response;
	}

	private async Task<DSMEnvelop<ExchangeRateModel, RateChangeFacade>> CreateResponse(decimal newRate, string source, ExchangeRateModel exchangeRates)
	{
		var response = DSMEnvelop<ExchangeRateModel, RateChangeFacade>.Initialize(_logger);

		try {
			response = IsUSDSource(source)
				? CreateDefaultResponse(newRate, exchangeRates)
				: await CreateSourceResponse(newRate, source, exchangeRates);

		} catch (Exception ex) {
			response.Error(ex);
		}

		return response;
	}

	private bool IsUSDSource(string source) {
		return (string.IsNullOrEmpty(source) || source.Equals("USD") || source.Equals("USDUSD"));
	}

	private async Task<DSMEnvelop<ExchangeRateModel, RateChangeFacade>> CreateSourceResponse(decimal newRate, string source, ExchangeRateModel exchangeRates)
	{
		var response = DSMEnvelop<ExchangeRateModel, RateChangeFacade>.Initialize(_logger);

		try
		{
			// Capture current rate of GB
			var officialResponse = await _upmaServices.GetOfficialPrice();
			if (officialResponse.Code != DSMEnvelopeCodeEnum._SUCCESS) return response.Rebase(officialResponse);
			var currentUSDGBRate = officialResponse.Payload;

			var acceptedKeys = GetCurrencyHashSetList();
			var filteredQuotes = exchangeRates.Quotes
				.Where(q => acceptedKeys.Contains(q.Key))
				.ToDictionary(q => q.Key, q => {
					if (q.Key.Equals(source)) return newRate;

					return q.Value * currentUSDGBRate;
				});

			filteredQuotes.Add("USDUSD", currentUSDGBRate);

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

	private DSMEnvelop<ExchangeRateModel, RateChangeFacade> CreateDefaultResponse(decimal rate, ExchangeRateModel exchangeRates)
	{
		var response = DSMEnvelop<ExchangeRateModel, RateChangeFacade>.Initialize(_logger);

		try {
			var acceptedKeys = GetCurrencyHashSetList();
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

	private HashSet<string> GetCurrencyHashSetList()
	{
		var currenciesListString = _configuration["CURRENCY_LIST"];
		if (string.IsNullOrEmpty(currenciesListString)) throw new Exception("Missing Currencies list");

		var acceptedListOfCurrencies = Newtonsoft.Json.JsonConvert.DeserializeObject<List<NameKeyModel>>(currenciesListString);
		if (acceptedListOfCurrencies == null) throw new Exception("No list of currencies is available");

		var acceptedKeys = acceptedListOfCurrencies.Select(c => c.Key).ToHashSet();

		return acceptedKeys;
	}

	public async Task<DSMEnvelop<IList<RateChangeDbEntity>,RateChangeFacade>> GetAllRateChanges(bool nuke = false)
	{
		var response = DSMEnvelop<IList<RateChangeDbEntity>,RateChangeFacade>.Initialize(_logger);

		try
		{
			var payloadResponse = await _rateChangeRepository.GetAllAsync();
			if (payloadResponse.Code != DSMEnvelopeCodeEnum._SUCCESS) return response.Rebase(payloadResponse);

			var googleSheetResponse = await _googleServices.UpdateRateChangeGoogleSheet(payloadResponse.Payload);
			if (googleSheetResponse.Code != DSMEnvelopeCodeEnum._SUCCESS) response.Rebase(googleSheetResponse);

			response.Success(payloadResponse.Payload);

		} catch(Exception ex) {
			response.Error(ex);
		}

		return response;
	}
}