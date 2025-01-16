using LimpezaDeBase.Infra.Interfaces;
using LimpezaDeBase.Modelos.BlipContatos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Text;

namespace LimpezaDeBase.Infra
{
    public class Blip : IBlip
    {
        private string _baseUrl;
        private HttpClient _httpClient;

        public Blip()
        {

        }

        public void CriarHttpClient(string contractId, string token)
        {
            _baseUrl = $"https://{contractId}.http.msging.net/commands";
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Key {token}");
            _httpClient.MaxResponseContentBufferSize = 50 * 1024 * 1024;
        }

        public async Task<BlipContatoResponse> GetContacts(int skip, int take)
        {
            var requestBody = new
            {
                id = Guid.NewGuid().ToString(),
                to = "postmaster@crm.msging.net",
                method = "get",
                uri = $"/contacts?$skip={skip}&$take={take}"
            };

            var jsonRequest = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync(_baseUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erro na requisição: {response.StatusCode}\n{errorContent}");
            }

            string responseBody = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<BlipContatoResponse>(responseBody);
        }
    }
}
