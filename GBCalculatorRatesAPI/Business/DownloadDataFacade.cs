namespace GBCalculatorRatesAPI.Business;

using GBCalculatorRatesAPI.Business.Models;
using GBCalculatorRatesAPI.Models;
using Microsoft.Extensions.Logging;
using QUAD.DSM;

public class DownloadDataFacade
{
	private readonly ILogger<DownloadDataFacade> _logger;

	public DownloadDataFacade(ILogger<DownloadDataFacade> logger)
	{
		_logger = logger;
	}
}