namespace GBCalculatorRatesAPI.Models;

using Newtonsoft.Json;

public class ExchangeRateModel {

	[JsonProperty("success")]
    public bool Success { get; set; }

    [JsonProperty("terms")]
	public required string Terms { get; set; }

    [JsonProperty("privacy")]
	public required string Privacy { get; set; }

	[JsonProperty("timestamp")]
    public required long Timestamp { get; set; } // number;

	[JsonProperty("source")]
	public required string Source { get; set; }

    [JsonProperty("quotes")]
    public required Dictionary<string, decimal> Quotes { get; set; }
}