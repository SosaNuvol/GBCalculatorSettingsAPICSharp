namespace GBCalculatorRatesAPI.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class SettingsDbEntity {
	[BsonId]
	[BsonElement("_id")]
	[BsonRepresentation(BsonType.ObjectId)]
 	public string _id { get; set; } = string.Empty;

    [BsonElement("version")]
    public string Version { get; set; } = string.Empty;

    [BsonElement("cacheExpiration")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CacheExpiration { get; set; }
}