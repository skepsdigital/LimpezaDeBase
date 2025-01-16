using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
namespace LimpezaDeBase.Modelos.Mongo
{
    public class ClienteDB
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string Nome { get; set; }
        public string Contrato { get; set; }
        public int Creditos { get; set; }
        public List<string>? Funcionalidades { get; set; }
    }
}
