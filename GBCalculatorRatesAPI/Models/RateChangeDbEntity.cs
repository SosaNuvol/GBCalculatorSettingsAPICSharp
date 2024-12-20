namespace GBCalculatorRatesAPI.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

[BsonIgnoreExtraElements]
public class GeoLocationPoint {
	[BsonElement("longitude")]
	double Longitude { get; set; }

	[BsonElement("latitude")]
	double Latitude { get; set; }
}

public interface IRateChange
{
	string Id { get; set; }
	string DeviceId { get; set; }
	decimal PreviousRate { get; set; }
	decimal CurrentRate { get; set; }
	GeoLocationPoint? LocationPoint { get; set; }
	string TimeStamp { get; set; }
}

public class GeoLocation : GeoLocationPoint
{
    public double Longitude { get; set; }
    public double Latitude { get; set; }
}

public class RateChangeRequest
{
	[JsonProperty("deviceId")]
    public required string DeviceId { get; set; }

	[JsonProperty("source")]
	public required string Source { get; set; }

	[JsonProperty("previousRate")]
    public decimal PreviousRate { get; set; }

	[JsonProperty("currentRate")]
    public decimal CurrentRate { get; set; }

	[JsonProperty("geoLocation")]
    public GeoLocation? GeoLocation { get; set; }
}

public class RateChangeDbEntity : IRateChange
{
    [BsonId]
    [BsonElement("_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("deviceId")]
    public required string DeviceId { get; set; }

    [BsonElement("source")]
    public required string Source { get; set; }

    [BsonElement("previousRate")]
    public required decimal PreviousRate { get; set; }

    [BsonElement("currentRate")]
    public required decimal CurrentRate { get; set; }

    [BsonElement("locationPoint")]
    public GeoLocationPoint? LocationPoint { get; set; }

    [BsonElement("timestamp")]
    public required string TimeStamp { get; set; }
}