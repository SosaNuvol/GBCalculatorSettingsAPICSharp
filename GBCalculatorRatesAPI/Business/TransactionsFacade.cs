namespace GBCalculatorRatesAPI.Business;

using GBCalculatorRatesAPI.Models;
using GBCalculatorRatesAPI.Repos;
using Microsoft.Extensions.Logging;
using QUAD.DSM;

public class TransactionsFacade
{
		private readonly ILogger<TransactionsFacade> _logger;

		private readonly LocationsRepository _locationsRepository;

		public TransactionsFacade(LocationsRepository locationsRepository, ILogger<TransactionsFacade> logger)
		{
			_logger = logger;
			_locationsRepository = locationsRepository;
		}

}