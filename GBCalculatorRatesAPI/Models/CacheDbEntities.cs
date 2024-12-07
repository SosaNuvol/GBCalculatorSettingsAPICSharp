namespace GBCalculatorRatesAPI.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class CacheDbEntity<T> {
	[BsonId]
	[BsonElement("_id")]
	[BsonRepresentation(BsonType.ObjectId)]
 	public string _id { get; set; } = string.Empty;

	[BsonElement("cacheType")]
	public required string CacheType { get; set; }

	[BsonElement("expiresAt")]
	// [BsonSerializer(typeof(DateTimeOffsetSerializer))]
	public required DateTimeOffset ExpiresAt { get; set; }

	[BsonElement("createdDate")]
	public required DateTimeOffset CreatedDate { get; set; }

    [BsonElement("payload")] // Generic payload to store serialized objects
    public required T Payload { get; set; }
}