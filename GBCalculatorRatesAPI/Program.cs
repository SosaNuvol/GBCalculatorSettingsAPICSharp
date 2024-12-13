using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using GBCalculatorRatesAPI.Services;
using Microsoft.Extensions.Configuration;
using GBCalculatorRatesAPI.Business;
using GBCalculatorRatesAPI.Repos;
using Microsoft.Extensions.Logging;
using GBCalculatorRatesAPI.Models;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services => {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
		services.AddHttpClient<GeocodingService>();
		services.AddSingleton(sp => new LocationsRepository(sp.GetRequiredService<IConfiguration>()));
		services.AddSingleton(sp => new RateChangeRepository(
			sp.GetRequiredService<ILogger<RateChangeRepository>>(), 
			sp.GetRequiredService<IConfiguration>()
		));
		services.AddSingleton(sp => new GeocodingService(
			sp.GetRequiredService<HttpClient>(),
			"AIzaSyAVNxM9pV4iKF2LowPGKfi8GGg4X0E11i8",
			sp.GetRequiredService<ILogger<GeocodingService>>()
		));
		services.AddSingleton(sp => new ExchangeServices(
			sp.GetRequiredService<HttpClient>(), 
			"AIzaSyAVNxM9pV4iKF2LowPGKfi8GGg4X0E11i8",
			sp.GetRequiredService<ILogger<ExchangeServices>>()
		));
		services.AddSingleton<LocationFacade>();
		services.AddSingleton<RateChangeFacade>();

        // Register CacheRepository and CacheFacade for ExchangeRateModel
        services.AddSingleton(sp => new CacheRepository<ExchangeRateModel>(
            sp.GetRequiredService<ILogger<CacheRepository<ExchangeRateModel>>>(),
            sp.GetRequiredService<IConfiguration>()
        ));
		services.AddSingleton(sp => new CacheFacade<ExchangeRateModel>(
			sp.GetRequiredService<ILogger<CacheFacade<ExchangeRateModel>>>(),
			sp.GetRequiredService<CacheRepository<ExchangeRateModel>>(),
			sp.GetRequiredService<ExchangeServices>()
		));
		
		services.AddLogging(loggingBuilder => {
            loggingBuilder.AddConsole();
            loggingBuilder.AddApplicationInsights();
        });
    })
	.ConfigureAppConfiguration((context, config) => {
		config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
	})
    .Build();

host.Run();
