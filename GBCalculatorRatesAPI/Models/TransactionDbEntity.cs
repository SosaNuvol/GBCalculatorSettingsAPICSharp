namespace GBCalculatorRatesAPI.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

public class TransactionDbEntity
{
	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	[JsonProperty("_id")]
	public required string Id { get; set; }

	[BsonElement("payloadVersion")]
	[JsonProperty("payloadVersion")]
	public required string PayloadVersion { get; set; }

	[BsonElement("deviceId")]
	[JsonProperty("deviceId")]
	public required string DeviceId { get; set; }

	[BsonElement("transactionDate")]
	[JsonProperty("transactionDate")]
	public DateTime TransactionDate { get; set; }

	[BsonElement("lastUpdateDate")]
	[JsonProperty("lastUpdateDate")]
	public DateTime LastUpdateDate { get; set; }

	[BsonElement("description")]
	[JsonProperty("description")]
	public required string Description { get; set; }

	[BsonElement("payload")]
	[JsonProperty("payload")]
	public required Payload Payload { get; set; }

	[BsonElement("coord")]
	[JsonProperty("coord")]
	public Coord? Coord { get; set; }
}

public class Payload
{
	[BsonElement("rate")]
	[JsonProperty("rate")]
	public double Rate { get; set; }

	[BsonElement("sourceCurrency")]
	[JsonProperty("sourceCurrency")]
	public required string SourceCurrency { get; set; }

	[BsonElement("targetCurrency")]
	[JsonProperty("targetCurrency")]
	public required string TargetCurrency { get; set; }

	[BsonElement("price")]
	[JsonProperty("price")]
	public int Price { get; set; }

	[BsonElement("due")]
	[JsonProperty("due")]
	public int Due { get; set; }

	[BsonElement("payment")]
	[JsonProperty("payment")]
	public int Payment { get; set; }

	[BsonElement("change")]
	[JsonProperty("change")]
	public double Change { get; set; }
}

public class Coord
{
	[BsonElement("latitude")]
	[JsonProperty("latitude")]
	public double Latitude { get; set; }

	[BsonElement("longitude")]
	[JsonProperty("longitude")]
	public double Longitude { get; set; }
}