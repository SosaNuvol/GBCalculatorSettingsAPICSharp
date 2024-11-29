namespace GBCalculatorRatesAPI.Utilities;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

public class NullableBooleanStringSerializer : SerializerBase<bool?>
{
    public override bool? Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var bsonType = context.Reader.GetCurrentBsonType();
        switch (bsonType)
        {
            case BsonType.Boolean:
                return context.Reader.ReadBoolean();
            case BsonType.String:
                var stringValue = context.Reader.ReadString().ToLower();
                return stringValue == "yes" || stringValue == "true";
            case BsonType.Null:
                context.Reader.ReadNull();
                return null;
            default:
                throw new BsonSerializationException($"Cannot deserialize BsonType {bsonType} to Nullable<Boolean>.");
        }
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, bool? value)
    {
        if (value.HasValue)
        {
            context.Writer.WriteBoolean(value.Value);
        }
        else
        {
            context.Writer.WriteNull();
        }
    }
}