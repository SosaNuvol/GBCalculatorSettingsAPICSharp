namespace GBCalculatorRatesAPI.Models;

using GBCalculatorRatesAPI.Utilities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;

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

	[BsonElement("businessName")]
	public string? BusinessName { get; set; }

	[BsonElement("businessCategory")]
	public string? BusinessCategory { get; set; }

	[BsonElement("businessDescription")]
	public string? BusinessDescription { get; set; }

	[BsonElement("businessWebAddress")]
	public string? BusinessWebAddress { get; set; }

	[BsonElement("businessPhone")]
	public string? BusinessPhone { get; set; }

	[BsonElement("businessAddress")]
	public string? BusinessAddress { get; set; }
	
	[BsonElement("businessLogoFileUrl")]
	public string? BusinessLogoFileUrl { get; set; }

	[BsonElement("submitedOn")]
	public string? SubmitedOn { get; set; }

	[BsonElement("yourName")]
	public string? YourName { get; set; }

	[BsonElement("yourPositionTitle")]
	public string? YourPositionTitle { get; set; }

	[BsonElement("yourPhone")]
	public string? YourPhone { get; set; }

	[BsonElement("yourEmail")]
	public string? YourEmail { get; set; }

	[BsonElement("showOnMap")]
	[BsonSerializer(typeof(NullableBooleanStringSerializer))]
	public bool? ShowOnMap { get; set; }

	[BsonElement("howDidYouHearAboutUs")]
	public string? HowDidYouHearAboutUs { get; set; }

	[BsonElement("source")]
	public string? Source { get; set; }

	[BsonElement("latitude")]
	public double? Latitude { get; set; }

	[BsonElement("longitude")]
	public double? Longitude { get; set; }

	[BsonElement("geoStatus")]
	public string GeoStatus { get; set; } = "[Not Set]";

	[BsonElement("geoMessage")]
	public string? GeoMessage { get; set; }

	[BsonElement("geoHouseNumber")]
	public string? GeoHouseNumber { get; set; }

    [BsonElement("geoZipCode")]
    public string? GeoZipCode { get; set; }

    [BsonElement("geoCity")]
    public string? GeoCity { get; set; }

    [BsonElement("geoStateProv")]
    public string? GeoStateProv { get; set; }

    [BsonElement("geoCounty")]
    public string? GeoCounty { get; set; }

    [BsonElement("geoCountry")]
    public string? GeoCountry { get; set; }

    [BsonElement("location")]
	public GeoJsonPoint<GeoJson2DGeographicCoordinates>? Location { get; set; }
}