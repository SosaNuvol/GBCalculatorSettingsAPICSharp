namespace GBCalculatorRatesAPI.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

[BsonIgnoreExtraElements]
public class GeoLocationPoint {
	[BsonElement("type")]
	public required string Type { get; set; }

	[BsonElement("coordinates")]
	public required double[] Coordinates { get; set; }

	// [BsonElement("longitude")]
	// double Longitude { get; set; }

	// [BsonElement("latitude")]
	// double Latitude { get; set; }
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
    private double _longitude;
    [JsonProperty("longitude")]
    public required double Longitude 
    { 
        get => _longitude; 
        set 
        {
            _longitude = value;
            UpdateCoordinates();
        }
    }

    private double _latitude;
    [JsonProperty("latitude")]
    public required double Latitude 
    { 
        get => _latitude; 
        set 
        {
            _latitude = value;
            UpdateCoordinates();
        }
    }

    public GeoLocation()
    {
        UpdateCoordinates();
    }

    private void UpdateCoordinates()
    {
        Coordinates = new[] { Longitude, Latitude };
    }
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

	[BsonElement("payloadVersion")]
	[JsonProperty("payloadVersion")]
	public required string PayloadVersion { get; set; }

    [BsonElement("deviceId")]
	[JsonProperty("deviceId")]
    public required string DeviceId { get; set; }

    [BsonElement("source")]
	[JsonProperty("source")]
    public required string Source { get; set; }

    [BsonElement("previousRate")]
    [JsonProperty("previousRate")]
    public required decimal PreviousRate { get; set; }

    [BsonElement("currentRate")]
    [JsonProperty("currentRate")]
    public required decimal CurrentRate { get; set; }

    [BsonElement("locationPoint")]
	[JsonProperty("locationPoint")]
    public GeoLocationPoint? LocationPoint { get; set; }

    [BsonElement("timestamp")]
	[JsonProperty("timestamp")]
    public required string TimeStamp { get; set; }
}