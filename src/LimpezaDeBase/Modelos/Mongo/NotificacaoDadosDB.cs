using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace LimpezaDeBase.Modelos.Mongo
{
    public class NotificacaoDadosDB
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string ProcessId { get; set; }
        public string Contrato { get; set; }
        public string KeyRoteador { get; set; }
        public string FlowId { get; set; }
        public string StateId { get; set; }
        public string MasterState { get; set; }
        public string Template { get; set; }
        public string NomeCampanha { get; set; }
        public string UrlFileOriginal { get; set; }
    }
}
