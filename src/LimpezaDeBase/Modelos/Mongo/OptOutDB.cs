using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace LimpezaDeBase.Modelos.Mongo
{
    public class OptOutDB
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public List<string> Telefone { get; set; } = new List<string>();
        public string Contrato { get; set; }
        public string Roteador { get; set; }
    }
}
