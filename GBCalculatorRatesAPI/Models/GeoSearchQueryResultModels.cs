namespace GBCalculatorRatesAPI.Models;

using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

public class LocationDocument: LocationDbEntity
{
	[BsonElement("distance"), JsonProperty("distance")]
    public double? Distance { get; set; }

}

// public class GeoLocationDocument
// {
//     public string Type { get; set; }
//     public List<double> Coordinates { get; set; }
// }

public class GroupedLocation
{
	[BsonElement("longitude"), JsonProperty("longitude")]
    public double Longitude { get; set; }

	[BsonElement("latitude"), JsonProperty("latitude")]
    public double Latitude { get; set; }

	[BsonElement("count"), JsonProperty("count")]
    public double Count { get; set; }

	[BsonElement("documents"), JsonProperty("documents")]
    public required List<LocationDocument> Documents { get; set; }
}

public class SingleLocation : LocationDocument
{
    // Inherits from LocationDocument
}

public class GeoSearchQueryResult
{
	[BsonElement("groupedLocations"), JsonProperty("groupedLocations")]
    public required List<GroupedLocation> GroupedLocations { get; set; }

	[BsonElement("singleLocations"), JsonProperty("singleLocations")]
    public required List<SingleLocation> SingleLocations { get; set; }
}
