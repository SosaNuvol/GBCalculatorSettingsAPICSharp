using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GBCalculatorRatesAPI
{
    public class CurrencyRates
    {
        private readonly ILogger<CurrencyRates> _logger;

        public CurrencyRates(ILogger<CurrencyRates> logger)
        {
            _logger = logger;
        }

        [Function("CurrencyRates")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
