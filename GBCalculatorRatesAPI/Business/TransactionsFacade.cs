namespace GBCalculatorRatesAPI.Business;

using GBCalculatorRatesAPI.Models;
using GBCalculatorRatesAPI.Repos;
using GBCalculatorRatesAPI.Services;
using Microsoft.Extensions.Logging;
using QUAD.DSM;

public class TransactionsFacade
{
	private readonly ILogger<TransactionsFacade> _logger;

	private readonly TransactionsRepository _transactionsRepository;

	private readonly GoogleServices _googleServices;

	public TransactionsFacade(TransactionsRepository transactionsRepository, GoogleServices googleServices, ILogger<TransactionsFacade> logger)
	{
		_logger = logger;
		_googleServices = googleServices;
		_transactionsRepository = transactionsRepository;
	}

	public async Task<DSMEnvelop<IList<TransactionDbEntity>,TransactionsFacade>> GetAllTransactions(bool nuke = false)
	{
		var response = DSMEnvelop<IList<TransactionDbEntity>,TransactionsFacade>.Initialize(_logger);

		try
		{
			var payloadResponse = await _transactionsRepository.GetAllAsync();
			if (payloadResponse.Code != DSMEnvelopeCodeEnum._SUCCESS) return response.Rebase(payloadResponse);

			var googleSheetResponse = await _googleServices.UpdateTransactionGoogleSheet(payloadResponse.Payload);
			if (googleSheetResponse.Code != DSMEnvelopeCodeEnum._SUCCESS) response.Rebase(googleSheetResponse);

			response.Success(payloadResponse.Payload);
		} catch (Exception ex) {
			response.Error(ex);
		}

		return response;
	}
}