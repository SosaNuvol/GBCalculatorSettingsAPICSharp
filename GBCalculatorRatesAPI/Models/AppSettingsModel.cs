using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace GBCalculatorRatesAPI.Models;

public class AppSettingsModel
{
	[JsonProperty("percentageChangeRate")]
	public required int PercentageChangeRate { get; set; } = 10;

	[JsonProperty("defaultSearchRadiusMiles")]
	public required int DefaultSearchRadius { get; set; } = 20;

	[JsonProperty("defaultStartingPoint")]
	public required GeoCircle DefaultStartingPoint { get; set; }
}

[BsonIgnoreExtraElements]
public class GeoCircle : GeoLocationPoint
{
	[BsonElement("radiusMiles")]
	[JsonProperty("radiusMiles")]
	public int RadiusMiles { get; set; }
}