namespace GBCalculatorRatesAPI.Utilities;

using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

public class DateTimeOffsetSerializer : SerializerBase<DateTimeOffset>
{
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateTimeOffset value)
    {
        context.Writer.WriteDateTime(value.UtcDateTime.Ticks);
    }

    public override DateTimeOffset Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var ticks = context.Reader.ReadDateTime();
        return new DateTimeOffset(new DateTime(ticks, DateTimeKind.Utc));
    }
}