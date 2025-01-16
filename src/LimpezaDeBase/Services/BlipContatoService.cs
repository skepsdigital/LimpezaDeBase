using LimpezaDeBase.Infra.Interfaces;
using LimpezaDeBase.Modelos;
using LimpezaDeBase.Modelos.BlipContatos;
using LimpezaDeBase.Services.Interfaces;
using System.Text;

namespace LimpezaDeBase.Services
{
    public class BlipContatoService : IBlipContatoService
    {
        private readonly IBlip _blip;

        public BlipContatoService(IBlip blip)
        {
            _blip = blip;
        }

        public async Task BuscarContatosBlip(string contractId, string token)
        {
            _blip.CriarHttpClient(contractId, token);

            int skip = 0;
            const int take = 30000;
            bool continuar = true;
            var contatos = new List<Contact>();

            while (continuar)
            {
                if (skip > 150000)
                {
                    continuar = false;
                }

                var response = await _blip.GetContacts(skip, take);

                if (response.Resource == null || response.Resource.Items == null || !response.Resource.Items.Any())
                {
                    continuar = false;
                }
                else
                {
                    contatos.AddRange(response.Resource.Items.Where(c => !string.IsNullOrWhiteSpace(c.name) && c.identity.Contains("@wa.gw.msging.net")));
                    skip += take;
                }

                await SalvarContatosEmCsv(contatos);
            }
        }
        private async Task SalvarContatosEmCsv(List<Contact> contatos)
        {
            var csvPath = "contatos_blip.csv"; // Nome do arquivo CSV
            var sb = new StringBuilder();

            // Cabeçalho do arquivo
            sb.AppendLine("Nome,Identidade");

            // Adiciona os contatos no formato CSV
            foreach (var contato in contatos)
            {
                sb.AppendLine($"{contato.name},{contato.identity}");
            }

            // Salvar o arquivo no disco
            await File.WriteAllTextAsync(csvPath, sb.ToString(), Encoding.UTF8);

            Console.WriteLine($"Arquivo CSV gerado com sucesso em: {csvPath}");
        }
    }
}
