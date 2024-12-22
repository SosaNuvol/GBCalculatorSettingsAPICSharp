namespace GBCalculatorRatesAPI.Business;

using Microsoft.Extensions.Logging;
using QUAD.DSM;

public class SettingsFacade
{
		private readonly ILogger<SettingsFacade> _logger;

		public SettingsFacade(ILogger<SettingsFacade> logger)
		{
			_logger = logger;
		}

		
}