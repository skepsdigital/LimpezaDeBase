using System.Text.Json.Serialization;

namespace LimpezaDeBase.Modelos
{
    public class ContatoCallbackResponse : Contato
    {
        [JsonPropertyName("has_whatsapp")]
        public bool? TemWhatsapp {  get; set; }

        [JsonPropertyName("request_id")]
        public string RequestId { get; set; }
    }
}
