using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace PersistedCache;

public class ExpireBsonSerializer : SerializerBase<Expire>
{
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Expire value)
    {
        var dateTimeOffset = (DateTimeOffset)value;
        context.Writer.WriteDateTime(dateTimeOffset.ToUnixTimeMilliseconds());
    }

    public override Expire Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var ticks = context.Reader.ReadDateTime();
        return DateTimeOffset.FromUnixTimeMilliseconds(ticks);
    }
}