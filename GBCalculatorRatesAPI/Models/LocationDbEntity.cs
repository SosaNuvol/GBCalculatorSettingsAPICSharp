namespace GBCalculatorRatesAPI.Models;

using GBCalculatorRatesAPI.Utilities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;
using Newtonsoft.Json;

public interface ILocation
{
	string _id { get; set; }
	int id { get; set; }
	string? BusinessName { get; set; }
	string? BusinessCategory { get; set; }
	string? BusinessDescription { get; set; }
	string? BusinessWebAddress { get; set; }
	string? BusinessPhone { get; set; }
	string? BusinessAddress { get; set; }
	string? BusinessLogoFileUrl { get; set; }
	string? SubmitedOn { get; set; }
	string? YourName { get; set; }
	string? YourPositionTitle { get; set; }
	string? YourPhone { get; set; }
	string? YourEmail { get; set; }
	bool? ShowOnMap { get; set; }
	string? HowDidYouHearAboutUs { get; set; }
	string? Source { get; set; }
	double? Latitude { get; set; }
	double? Longitude { get; set; }
	string GeoStatus { get; set; }
	string? GeoMessage { get; set; }
	string? GeoHouseNumber { get; set; }
	string? GeoZipCode { get; set; }
	string? GeoCity { get; set; }
	string? GeoStateProv { get; set; }
	string? GeoCounty { get; set; }
	string? GeoCountry { get; set; }
}

public class LocationDbEntity: ILocation
{
	[BsonId]
	[BsonElement("_id")]
	[BsonRepresentation(BsonType.ObjectId)]
 	public string _id { get; set; } = string.Empty;

	[BsonElement("id")]
	public int id { get; set; }

	[BsonElement("businessName"), JsonProperty("businessName")]
	public string? BusinessName { get; set; }

	[BsonElement("businessCategory"), JsonProperty("businessCategory")]
	public string? BusinessCategory { get; set; }

	[BsonElement("businessDescription"), JsonProperty("businessDescription")]
	public string? BusinessDescription { get; set; }

	[BsonElement("businessWebAddress"), JsonProperty("businessWebAddress")]
	public string? BusinessWebAddress { get; set; }

	[BsonElement("businessPhone"), JsonProperty("businessPhone")]
	public string? BusinessPhone { get; set; }

	[BsonElement("businessAddress"), JsonProperty("businessAddress")]
	public string? BusinessAddress { get; set; }
	
	[BsonElement("businessLogoFileUrl"), JsonProperty("businessLogoFileUrl")]
	public string? BusinessLogoFileUrl { get; set; }

	[BsonElement("submitedOn"), JsonProperty("submitedOn")]
	public string? SubmitedOn { get; set; }

	[BsonElement("yourName"), JsonProperty("yourName")]
	public string? YourName { get; set; }

	[BsonElement("yourPositionTitle"), JsonProperty("yourPositionTitle")]
	public string? YourPositionTitle { get; set; }

	[BsonElement("yourPhone"), JsonProperty("yourPhone")]
	public string? YourPhone { get; set; }

	[BsonElement("yourEmail"), JsonProperty("yourEmail")]
	public string? YourEmail { get; set; }

	[BsonElement("showOnMap"), JsonProperty("showOnMap")]
	[BsonSerializer(typeof(NullableBooleanStringSerializer))]
	public bool? ShowOnMap { get; set; }

	[BsonElement("howDidYouHearAboutUs"), JsonProperty("howDidYouHearAboutUs")]
	public string? HowDidYouHearAboutUs { get; set; }

	[BsonElement("source"), JsonProperty("source")]
	public string? Source { get; set; }

	[BsonElement("latitude"), JsonProperty("latitude")]
	public double? Latitude { get; set; }

	[BsonElement("longitude"), JsonProperty("longitude")]
	public double? Longitude { get; set; }

	[BsonElement("geoStatus"), JsonProperty("geoStatus")]
	public string GeoStatus { get; set; } = "[Not Set]";

	[BsonElement("geoMessage"), JsonProperty("geoMessage")]
	public string? GeoMessage { get; set; }

	[BsonElement("geoHouseNumber"), JsonProperty("geoHouseNumber")]
	public string? GeoHouseNumber { get; set; }

    [BsonElement("geoZipCode"), JsonProperty("geoZipCode")]
    public string? GeoZipCode { get; set; }

    [BsonElement("geoCity"), JsonProperty("geoCity")]
    public string? GeoCity { get; set; }

    [BsonElement("geoStateProv"), JsonProperty("geoStateProv")]
    public string? GeoStateProv { get; set; }

    [BsonElement("geoCounty"), JsonProperty("geoCounty")]
    public string? GeoCounty { get; set; }

    [BsonElement("geoCountry"), JsonProperty("geoCountry")]
    public string? GeoCountry { get; set; }

    [BsonElement("location"), JsonProperty("location")]
	public GeoJsonPoint<GeoJson2DGeographicCoordinates>? Location { get; set; }
}