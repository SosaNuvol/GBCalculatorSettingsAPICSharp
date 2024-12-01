using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using GBCalculatorRatesAPI.Services;
using Microsoft.Extensions.Configuration;
using GBCalculatorRatesAPI.Business;
using GBCalculatorRatesAPI.Repos;
using DnsClient.Internal;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services => {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
		services.AddHttpClient<GeocodingService>();
		services.AddSingleton(sp => new LocationsRepository(sp.GetRequiredService<IConfiguration>()));
		services.AddSingleton(sp => new GeocodingService(sp.GetRequiredService<HttpClient>(), "AIzaSyAVNxM9pV4iKF2LowPGKfi8GGg4X0E11i8"));
		services.AddSingleton<LocationFacade>();
		// services.AddSingleton(sp => new LocationFacade(
		// 	sp.GetRequiredService<ILoggerFactory>(),
		// 	sp.GetRequiredService<LocationsRepository>(), 
		// 	sp.GetRequiredService<GeocodingService>()
		// ));
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
