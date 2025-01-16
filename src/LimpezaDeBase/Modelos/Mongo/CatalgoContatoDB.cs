using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace LimpezaDeBase.Modelos.Mongo
{
    public class CatalgoContatoDB
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public Dictionary<string, bool> Numeros { get; set; }
    }
}
