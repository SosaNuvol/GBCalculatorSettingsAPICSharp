using Newtonsoft.Json;

namespace GBCalculatorRatesAPI.Models;
public interface IUPMAPayload
{
	string GoldSpot { get; set; }
	string GoldRate { get; set; }
	string GoldBuyBack { get; set; }
	string DayOfRate { get; set; }
	string SilverSpot { get; set; }
	string SilverRate { get; set; }
	string SilverBuyBack { get; set; }
	string GoldbackOfficialPrice { get; set; }
	string GoldbackRate { get; set; }
	string GoldbackBuyBack { get; set; }
}

public class UPMAPayload : IUPMAPayload {

	[JsonProperty("gold_spot")]
	public required string GoldSpot { get; set; }

	[JsonProperty("gold_rate")]
	public required string GoldRate { get; set; }

	[JsonProperty("gold_buy_back")]
	public required string GoldBuyBack { get; set; }

	[JsonProperty("day_of_rate")]
	public required string DayOfRate { get; set; }

	[JsonProperty("silver_spot")]
	public required string SilverSpot { get; set; }

	[JsonProperty("silver_rate")]
	public required string SilverRate { get; set; }

	[JsonProperty("silver_buy_back")]
	public required string SilverBuyBack { get; set; }

	[JsonProperty("goldback_official_price")]
	public required string GoldbackOfficialPrice { get; set; }

	[JsonProperty("goldback_rate")]
	public required string GoldbackRate { get; set; }

	[JsonProperty("goldback_buy_back")]
	public required string GoldbackBuyBack { get; set; }
}