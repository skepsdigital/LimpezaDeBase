using System.Text.Json.Serialization;

namespace LimpezaDeBase.Modelos.Notify
{
    public class SummaryRequest
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("to")]
        public string To { get; set; } = "postmaster@activecampaign.msging.net";

        [JsonPropertyName("method")]
        public string Method { get; set; } = "get";

        [JsonPropertyName("uri")]
        public string Uri { get; set; }
    }
}
