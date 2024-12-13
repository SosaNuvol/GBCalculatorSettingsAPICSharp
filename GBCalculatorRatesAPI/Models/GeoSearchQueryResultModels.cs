namespace GBCalculatorRatesAPI.Models;

using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class LocationDocument: LocationDbEntity
{
	/*
    [BsonId]
    public ObjectId Id { get; set; }

    public int IdNumber { get; set; }
    public string BusinessName { get; set; }
    public string BusinessCategory { get; set; }
    public string BusinessDescription { get; set; }
    public string BusinessWebAddress { get; set; }
    public string BusinessPhone { get; set; }
    public string BusinessAddress { get; set; }
    public string BusinessLogoFileUrl { get; set; }
    public string SubmitedOn { get; set; }
    public string YourName { get; set; }
    public string YourPositionTitle { get; set; }
    public string YourPhone { get; set; }
    public string YourEmail { get; set; }
    public bool? ShowOnMap { get; set; }
    public string HowDidYouHearAboutUs { get; set; }
    public string Source { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string GeoStatus { get; set; }
    public string GeoMessage { get; set; }
    public string GeoHouseNumber { get; set; }
    public string GeoZipCode { get; set; }
    public string GeoCity { get; set; }
    public string GeoStateProv { get; set; }
    public string GeoCounty { get; set; }
    public string GeoCountry { get; set; }
    public GeoLocationDocument Location { get; set; }
	*/
	[BsonElement("distance")]
    public double? Distance { get; set; }

}

// public class GeoLocationDocument
// {
//     public string Type { get; set; }
//     public List<double> Coordinates { get; set; }
// }

public class GroupedLocation
{
	[BsonElement("longitude")]
    public double Longitude { get; set; }

	[BsonElement("latitude")]
    public double Latitude { get; set; }

	[BsonElement("count")]
    public double Count { get; set; }

	[BsonElement("documents")]
    public required List<LocationDocument> Documents { get; set; }
}

public class SingleLocation : LocationDocument
{
    // Inherits from LocationDocument
}

public class GeoSearchQueryResult
{
	[BsonElement("groupedLocations")]
    public required List<GroupedLocation> GroupedLocations { get; set; }

	[BsonElement("singleLocations")]
    public required List<SingleLocation> SingleLocations { get; set; }
}
