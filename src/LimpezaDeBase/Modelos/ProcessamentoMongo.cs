using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace LimpezaDeBase.Modelos
{
    public class ProcessamentoMongo
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string ProcessId { get; set; }
        public int NumeroContatosRequest { get; set; }
        public string Email { get; set; }

        public string Contrato { get; set; }
    }
}
