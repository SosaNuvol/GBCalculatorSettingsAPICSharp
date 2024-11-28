using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using GBCalculatorRatesAPI.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services => {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
		services.AddHttpClient<GeocodingService>();
		services.AddSingleton(sp => new GeocodingService(sp.GetRequiredService<HttpClient>(), "AIzaSyAVNxM9pV4iKF2LowPGKfi8GGg4X0E11i8"));
    })
    .Build();

host.Run();
