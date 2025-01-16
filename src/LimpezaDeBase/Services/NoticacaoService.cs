using LimpezaDeBase.Extension;
using LimpezaDeBase.Infra.Interfaces;
using LimpezaDeBase.Modelos.Entidades;
using LimpezaDeBase.Modelos.Notify;
using LimpezaDeBase.Services.Interfaces;
using RestEase;
using System.Text.Json;

namespace LimpezaDeBase.Services
{
    public class NoticacaoService : INotificacaoService
    {
        private readonly IMongoService _mongoService;
        private readonly ICsvService _csvService;
        private readonly IAwsS3 _awsS3Service;
        private readonly ILogger<NoticacaoService> _logger;

        public NoticacaoService(IMongoService mongoService, IAwsS3 awsS3Service, ICsvService csvService, ILogger<NoticacaoService> logger)
        {
            _mongoService = mongoService;
            _awsS3Service = awsS3Service;
            _csvService = csvService;
            _logger = logger;
        }

        public async Task<dynamic> DispararNotificacoes(string processId, List<TelefoneEntity> telefonesProcessados)
        {
            try
            {
                var bytesCsv = await ObterArquivoOriginal(processId);
                List<dynamic> telefonesOriginais = await _csvService.LerCsvComoDinamico(bytesCsv);

                var notificacaoDados = (await _mongoService.ObterTodosDadosDeNotificacaoAsync()).FirstOrDefault(n => n.ProcessId.Equals(processId));
                var audiencia = new List<Audience>();
                var sender = RestClient.For<INotificationSender>($"https://{notificacaoDados.Contrato}.http.msging.net");

                foreach (var telefone in telefonesProcessados)
                {
                    if (telefone.PossuiWpp == false)
                        continue;

                    dynamic telefoneOriginal;
                    try
                    {
                        telefoneOriginal = telefonesOriginais.First(t =>
                        {
                            string telefoneStr = t.Telefone as string;
                            return telefoneStr.NormalizarTelefone() == telefone.Telefone;
                        });
                    }
                    catch (Exception)
                    {
                        telefoneOriginal = telefonesOriginais.First(t =>
                        {
                            string telefoneStr = t.telefone as string;
                            return telefoneStr.NormalizarTelefone() == telefone.Telefone;
                        });
                    }

                    if (telefoneOriginal != null)
                    {
                        var messageParams = new Dictionary<string, string>();
                        int index = 1;

                        // Iterar pelas propriedades do objeto dinâmico
                        foreach (var property in (IDictionary<string, object>)telefoneOriginal)
                        {
                            if (property.Key.Equals("telefone", StringComparison.OrdinalIgnoreCase))
                                continue;

                            messageParams[index.ToString()] = property.Value?.ToString() ?? string.Empty;
                            index++;
                        }

                        audiencia.Add(new Audience
                        {
                            Recipient = "+" + telefone.Telefone,
                            MessageParams = messageParams.Any() ? messageParams : null
                        });
                    }
                }


                var campaingRequest = new CampaignRequest()
                {
                    Id = Guid.NewGuid().ToString(),
                    Method = "SET",
                    To = "postmaster@activecampaign.msging.net",
                    Uri = "/campaign/full",
                    Type = "application/vnd.iris.activecampaign.full-campaign+json",
                    Resource = new Resource()
                    {
                        Campaign = new Campaign()
                        {
                            Name = notificacaoDados.NomeCampanha,
                            CampaignType = "Batch",
                            FlowId = notificacaoDados.FlowId,
                            StateId = notificacaoDados.StateId,
                            MasterState = notificacaoDados.MasterState + "@msging.net",
                            ChannelType = "WhatsApp"
                        },
                        Message = new Message()
                        {
                            MessageTemplate = notificacaoDados.Template,
                            MessageParams = audiencia.First().MessageParams is not null
                                            ? audiencia.First().MessageParams.Select(d => d.Key).ToList()
                                            : null,
                            ChannelType = "WhatsApp"
                        },
                        Audiences = audiencia
                    }
                };

                var jsonRequest = JsonSerializer.Serialize(campaingRequest);

                return await sender.Send(notificacaoDados.KeyRoteador, jsonRequest);
            }
            catch(Exception ex)
            {
                _logger.LogError($"Erro ao enviar notificações - {ex.Message}");
                return null;
            }
        }

        private async Task<byte[]> ObterArquivoOriginal(string processId)
        {
            var arquivoNome = $"fileOriginal-{processId}";

            return await _awsS3Service.GetFileAsync(arquivoNome);
        }
    }
}
