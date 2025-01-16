using System.Text.Json.Serialization;

namespace LimpezaDeBase.Modelos
{
    public class ContatoRequestOtima
    {
        [JsonPropertyName("contacts")]
        public List<Contato> Contacts { get; set; }

        [JsonPropertyName("callback_url")]
        public string CallbackUrl { get; set; }

        [JsonPropertyName("check_whatsapp")]
        public bool CheckWhatsapp { get; set; }
    }
}
