using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace LimpezaDeBase.Modelos
{
    public class ContatoCallbackObjetoMongo
    {
        [BsonId] 
        public ObjectId Id { get; set; }

        public string ProcessId { get; set; }

        public List<ContatoCallbackResponse> ContatosCallback { get; set; } = new List<ContatoCallbackResponse>();
        
        public int NumeroContatoRequest { get; set; }
        public string AgenteLimpeza { get; set; }
    }
}
