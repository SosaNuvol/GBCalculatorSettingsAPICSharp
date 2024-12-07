namespace GBCalculatorRatesAPI.Business;

using Microsoft.Extensions.Logging;

public class SettingsRepository {
	private readonly ILogger<SettingsRepository> _logger;

	public SettingsRepository(ILogger<SettingsRepository> logger) {
		_logger = logger;
	}
}