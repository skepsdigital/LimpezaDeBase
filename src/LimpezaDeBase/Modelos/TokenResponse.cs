using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LimpezaDeBase.Modelos
{
    public class TokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expire_in")]
        public string ExpiresIn { get; set; }
    }

}
