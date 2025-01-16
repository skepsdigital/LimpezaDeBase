using System.Text.Json.Serialization;

namespace LimpezaDeBase.Modelos.Notify
{
    public class CampaignRequest
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("to")]
        public string To { get; set; }

        [JsonPropertyName("method")]
        public string Method { get; set; }

        [JsonPropertyName("uri")]
        public string Uri { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("resource")]
        public Resource Resource { get; set; }
    }

    public class Resource
    {
        [JsonPropertyName("campaign")]
        public Campaign Campaign { get; set; }

        [JsonPropertyName("audiences")]
        public List<Audience> Audiences { get; set; }

        [JsonPropertyName("message")]
        public Message Message { get; set; }
    }

    public class Campaign
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("campaignType")]
        public string CampaignType { get; set; }

        [JsonPropertyName("flowId")]
        public string FlowId { get; set; }

        [JsonPropertyName("stateId")]
        public string StateId { get; set; }

        [JsonPropertyName("masterState")]
        public string MasterState { get; set; }

        [JsonPropertyName("channelType")]
        public string ChannelType { get; set; }
    }

    public class Audience
    {
        [JsonPropertyName("recipient")]
        public string Recipient { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("messageParams")]
        public Dictionary<string, string>? MessageParams { get; set; }
    }

    public class Message
    {
        [JsonPropertyName("messageTemplate")]
        public string MessageTemplate { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("messageParams")]
        public List<string>? MessageParams { get; set; }

        [JsonPropertyName("channelType")]
        public string ChannelType { get; set; }
    }
}
