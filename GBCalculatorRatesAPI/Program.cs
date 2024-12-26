using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using GBCalculatorRatesAPI.Services;
using Microsoft.Extensions.Configuration;
using GBCalculatorRatesAPI.Business;
using GBCalculatorRatesAPI.Repos;
using Microsoft.Extensions.Logging;
using GBCalculatorRatesAPI.Models;
using dotenv.net;

public class Program
{
    public static void Main(string[] args)
    {
		// Load envitonment variables from .env file
		DotEnv.Load();

        var googleApiKey = Environment.GetEnvironmentVariable("GoogleApiKey");
		var currencyExchnageApiKey = Environment.GetEnvironmentVariable("CURRENCY_EXCHANGE_KEY") ?? "3ab01cee0923aaf526a957a3b5ba8c31";

        if (string.IsNullOrEmpty(googleApiKey))
        {
            throw new InvalidOperationException("GoogleApiKey environment variable is not set.");
        }

		var host = new HostBuilder()
			.ConfigureFunctionsWebApplication()
			.ConfigureServices(services => {
				services.AddApplicationInsightsTelemetryWorkerService();
				services.ConfigureFunctionsApplicationInsights();
				services.AddHttpClient<GeocodingService>();
				
				services.AddSingleton<CacheRepository<AppSettingsModel>>();
				
				services.AddSingleton<CacheFacade<AppSettingsModel>>();
				services.AddSingleton<SettingsFacade>();

				services.AddSingleton<CacheRepository<ExchangeRateModel>>();
				services.AddSingleton<CacheFacade<ExchangeRateModel>>();
        
				// Register ExchangeServices with required string parameter
				services.AddSingleton(sp => new ExchangeServices(
					sp.GetRequiredService<HttpClient>(), 
					currencyExchnageApiKey,
					sp.GetRequiredService<ILogger<ExchangeServices>>()
				));
        
				// Register UPMAServices
				services.AddSingleton<UPMAServices>();
				
				services.AddSingleton(sp => new LocationsRepository(
					sp.GetRequiredService<ILogger<LocationsRepository>>(),
					sp.GetRequiredService<IConfiguration>()
				));
				services.AddSingleton(sp => new TransactionsRepository(
					sp.GetRequiredService<ILogger<TransactionsRepository>>(),
					sp.GetRequiredService<IConfiguration>()
				));
				services.AddSingleton(sp => new RateChangeRepository(
					sp.GetRequiredService<ILogger<RateChangeRepository>>(),
					sp.GetRequiredService<IConfiguration>()
				));
				services.AddSingleton(sp => new GeocodingService(
					sp.GetRequiredService<HttpClient>(),
					googleApiKey,
					sp.GetRequiredService<ILogger<GeocodingService>>()
				));
				services.AddSingleton(sp => new GoogleServices(
					googleApiKey,
					sp.GetRequiredService<ILogger<GoogleServices>>(),
					sp.GetRequiredService<IConfiguration>()
				));
				services.AddSingleton<LocationFacade>();
				services.AddSingleton<TransactionsFacade>();
				services.AddSingleton<DownloadDataFacade>();
				services.AddSingleton<RateChangeFacade>();
			})
			.Build();
			// .ConfigureServices(services => {
			// 	services.AddApplicationInsightsTelemetryWorkerService();
			// 	services.ConfigureFunctionsApplicationInsights();
			// 	services.AddHttpClient<GeocodingService>();
			// 	// Register CacheRepository<AppSettingsModel>
			// 	services.AddSingleton<CacheRepository<AppSettingsModel>>();
				
			// 	// Register CacheFacade<AppSettingsModel> and its dependencies
			// 	services.AddSingleton<CacheFacade<AppSettingsModel>>();
			// 	services.AddSingleton<SettingsFacade>();
			// 	services.AddSingleton(sp => new LocationsRepository(
			// 		sp.GetRequiredService<ILogger<LocationsRepository>>(),
			// 		sp.GetRequiredService<IConfiguration>()
			// 	));
			// 	services.AddSingleton(sp => new TransactionsRepository(
			// 		sp.GetRequiredService<ILogger<TransactionsRepository>>(),
			// 		sp.GetRequiredService<IConfiguration>()
			// 	));
			// 	services.AddSingleton(sp => new RateChangeRepository(
			// 		sp.GetRequiredService<ILogger<RateChangeRepository>>(),
			// 		sp.GetRequiredService<IConfiguration>()
			// 	));
			// 	services.AddSingleton(sp => new GeocodingService(
			// 		sp.GetRequiredService<HttpClient>(),
			// 		googleApiKey,
			// 		sp.GetRequiredService<ILogger<GeocodingService>>()
			// 	));
			// 	services.AddSingleton(sp => new GoogleServices(
			// 		googleApiKey,
			// 		sp.GetRequiredService<ILogger<GoogleServices>>(),
			// 		sp.GetRequiredService<IConfiguration>()
			// 	));
			// 	services.AddSingleton(sp => new UPMAServices(
			// 		sp.GetRequiredService<HttpClient>(),
			// 		sp.GetRequiredService<IConfiguration>(),
			// 		sp.GetRequiredService<ILogger<UPMAServices>>()
			// 	));
			// 	services.AddSingleton(sp => new ExchangeServices(
			// 		sp.GetRequiredService<HttpClient>(), 
			// 		currencyExchnageApiKey,
			// 		sp.GetRequiredService<ILogger<ExchangeServices>>()
			// 	));

			// 	services.AddSingleton(sp => new TransactionsFacade(
			// 		sp.GetRequiredService<TransactionsRepository>(),
			// 		sp.GetRequiredService<GoogleServices>(),
			// 		sp.GetRequiredService<ILogger<TransactionsFacade>>()
			// 	));
			// 	services.AddSingleton<LocationFacade>();
			// 	services.AddSingleton<RateChangeFacade>();
			// 	services.AddSingleton<SettingsFacade>();

			// 	// Register CacheRepository and CacheFacade for ExchangeRateModel
			// 	services.AddSingleton(sp => new CacheRepository<ExchangeRateModel>(
			// 		sp.GetRequiredService<ILogger<CacheRepository<ExchangeRateModel>>>(),
			// 		sp.GetRequiredService<IConfiguration>()
			// 	));
			// 	services.AddSingleton(sp => new CacheFacade<ExchangeRateModel>(
			// 		sp.GetRequiredService<ILogger<CacheFacade<ExchangeRateModel>>>(),
			// 		sp.GetRequiredService<CacheRepository<ExchangeRateModel>>(),
			// 		sp.GetRequiredService<ExchangeServices>(),
			// 		sp.GetRequiredService<UPMAServices>(),
			// 		sp.GetRequiredService<SettingsFacade>()
			// 	));
				
			// 	services.AddLogging(loggingBuilder => {
			// 		loggingBuilder.AddConsole();
			// 		loggingBuilder.AddApplicationInsights();
			// 	});
			// })
			// .ConfigureAppConfiguration((context, config) => {
			// 	config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
			// })
			// .Build();

		host.Run();
    }
}