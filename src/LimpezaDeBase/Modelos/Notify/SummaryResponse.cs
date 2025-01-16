using System.Text.Json.Serialization;

namespace LimpezaDeBase.Modelos.Notify
{
    public class SummaryResponse
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("resource")]
        public ResourceSummary Resource { get; set; }

        [JsonPropertyName("method")]
        public string Method { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("from")]
        public string From { get; set; }

        [JsonPropertyName("to")]
        public string To { get; set; }

        [JsonPropertyName("metadata")]
        public Metadata Metadata { get; set; }
    }

    public class ResourceSummary
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("itemType")]
        public string ItemType { get; set; }

        [JsonPropertyName("items")]
        public List<CampaignSummaryItem> Items { get; set; }
    }

    public class CampaignSummaryItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("messageTemplate")]
        public string MessageTemplate { get; set; }

        [JsonPropertyName("sendDate")]
        public DateTime SendDate { get; set; }

        [JsonPropertyName("statusAudience")]
        public List<StatusAudience> StatusAudience { get; set; }
    }

    public class StatusAudience
    {
        [JsonPropertyName("RecipientIdentity")]
        public string RecipientIdentity { get; set; }

        [JsonPropertyName("Status")]
        public string Status { get; set; }

        [JsonPropertyName("ReasonCode")]
        public int? ReasonCode { get; set; }

        [JsonPropertyName("ReasonDescription")]
        public string ReasonDescription { get; set; }
    }

    public class Metadata
    {
        [JsonPropertyName("#command.uri")]
        public string CommandUri { get; set; }

        [JsonPropertyName("uber-trace-id")]
        public string UberTraceId { get; set; }
    }
}
