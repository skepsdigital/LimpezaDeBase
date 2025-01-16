using LimpezaDeBase.Infra.Interfaces;
using LimpezaDeBase.Modelos.Mongo;
using LimpezaDeBase.Modelos.Notify;
using LimpezaDeBase.Services.Interfaces;
using RestEase;
using System.Diagnostics;
using System.Text.Json;

namespace LimpezaDeBase.Services
{
    public class RelatorioService : IRelatorioService
    {
        private readonly IRelatorioRepository _relatorioRepository;
        private readonly IMongoService _mongoService;

        public RelatorioService(IRelatorioRepository relatorioRepository, IMongoService mongoService)
        {
            _relatorioRepository = relatorioRepository;
            _mongoService = mongoService;
        }

        public async Task<Dictionary<string, int>> ObterStatusCampanhaNotificacao(string idProcessamento)
        {
            try
            {
                var relatorio = (await _relatorioRepository.BuscarPorProcessamentoAsync(idProcessamento)).First();
                var notificacaoDados = (await _mongoService.ObterTodosDadosDeNotificacaoAsync()).FirstOrDefault(n => n.ProcessId.Equals(idProcessamento));

                var sender = RestClient.For<INotificationSender>($"https://{notificacaoDados.Contrato}.http.msging.net");

                var summaryRequest = new SummaryRequest()
                {
                    Uri = $"/campaigns/{relatorio.IdNotificacoes}/summaries"
                };

                var jsonRequest = JsonSerializer.Serialize(summaryRequest);

                var response = await sender.GetSumary(notificacaoDados.KeyRoteador, jsonRequest);

                return response.Resource.Items.First().StatusAudience.GroupBy(sa => sa.Status)
                    .ToDictionary(g => g.Key, g => g.Count());
            }
            catch(Exception ex)
            {
                return null;
            }
        }
    }
}
