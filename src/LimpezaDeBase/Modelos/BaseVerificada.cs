using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using LimpezaDeBase.Modelos.Entidades;

namespace LimpezaDeBase.Modelos
{
    public class BaseVerificada
    {
        [BsonId] 
        public ObjectId Id { get; set; }

        public string ProcessId { get; set; }

        public List<TelefoneEntity> TelefonesVerificados { get; set; } = new List<TelefoneEntity>();
        
        public string AgenteDeLimpeza { get; set; }
    }
}
