using LimpezaDeBase.Extension;
using LimpezaDeBase.Infra.Interfaces;
using LimpezaDeBase.Limpeza.Interfaces;
using LimpezaDeBase.Modelos;
using LimpezaDeBase.Modelos.Entidades;
using LimpezaDeBase.Services.Interfaces;
using System.Text.Json;

namespace LimpezaDeBase.Limpeza.Strategy
{
    public class LimparWhatsappStrategy : ILimpezaStrategy
    {
        private readonly ITelefoneRepository _telefoneRepository;
        private readonly IProcessamentoService _processamentoService;
        private readonly IProcessamentoRepository _processamentoRepository;
        private readonly IOtimaAPI _otimaAPI;
        private readonly IMongoService _mongoService;

        private readonly ILogger<LimparWhatsappStrategy> _logger;

        private Dictionary<string, string> _credenciais;

        private const int MAX_CONTACT_REQUEST = 3000;
        private const string BASE_URL_CALLBACK = "https://d46pjmuoog.execute-api.sa-east-1.amazonaws.com/Prod/limpar/callback/{0}";

        public LimparWhatsappStrategy(ITelefoneRepository telefoneRepository, IProcessamentoRepository processamentoRepository, IOtimaAPI otimaAPI, Dictionary<string, string> credenciais, IMongoService mongoService, ILogger<LimparWhatsappStrategy> logger, IProcessamentoService processamentoService)
        {
            _telefoneRepository = telefoneRepository;
            _processamentoRepository = processamentoRepository;
            _otimaAPI = otimaAPI;
            _credenciais = credenciais;
            _mongoService = mongoService;
            _logger = logger;
            _processamentoService = processamentoService;
        }

        public async Task Validar(List<Contato> contatos, string processId, string contrato, string roteador, string email, int numeroDeContatos, bool enviarNotificacao, List<Contato>? contatoExclusao = null)
        {
            await _processamentoRepository.AdicionarAsync(new ProcessamentoEntity
            {
                Contrato = contrato,
                Roteador = roteador,
                Data = DateTime.Now,
                Email = email,
                FoiProcessado = false,
                NumeroDeContatosTotal = numeroDeContatos,
                ProcessId = processId.ToString(),
                EnviarNotificacao = enviarNotificacao,
                Funcionalidade = "limpezaDeBase"
            });

            await LimparOptOut(contatos, contrato, roteador, processId);
            await LimparInternamente(contatos, processId, roteador);
            await LimparExternamente(contatos, processId);

            await _processamentoService.EnviarEmailAcompanhamento(email, processId);
        }


        private async Task LimparOptOut(List<Contato> contatos, string contrato, string roteador, string processId)
        {
            var baseVerificada = new BaseVerificada();

            var optouts = (await _mongoService.ObterOptOutPorContrato(contrato)).FirstOrDefault(o => o.Roteador == roteador);

            if (optouts is null)
            {
                return;
            }

            baseVerificada.ProcessId = processId;
            baseVerificada.AgenteDeLimpeza = "SkepsOptOut";

            _logger.LogInformation($"{optouts.Telefone.Count} de optouts foram encontrados - {processId.ToString()}");

            foreach (var contatoUnico in contatos)
            {
                if (optouts.Telefone.Contains(contatoUnico.Telefone))
                {
                    baseVerificada.TelefonesVerificados.Add(new TelefoneEntity()
                    {
                        Telefone = contatoUnico.Telefone,
                        PossuiWpp = false
                    });
                }
            }

            var telefonesVerificados = baseVerificada.TelefonesVerificados.Select(t => t.Telefone).ToHashSet();
            contatos.RemoveAll(c => telefonesVerificados.Contains(c.Telefone));

            if (baseVerificada.TelefonesVerificados.Count != 0)
            {
                await _mongoService.AdicionarDocumentoAsync(baseVerificada);
            }
        }

        private async Task LimparInternamente(List<Contato> contatos, string processId, string roteador)
        {
            _logger.LogInformation("Iniciando limpeza interna");

            try
            {
                var baseVerificada = new BaseVerificada();

                baseVerificada.ProcessId = processId;
                baseVerificada.AgenteDeLimpeza = "Skeps";

                baseVerificada.TelefonesVerificados.AddRange(contatos.FiltrarTelefonesInvalidos()
                                                                     .Select(c => new TelefoneEntity()
                                                                     {
                                                                         Telefone = c.Telefone,
                                                                         PossuiWpp = false
                                                                     }).ToList());

                var result = await _telefoneRepository.PesquisarNumerosAsync(contatos.Select(c => c.Telefone).ToList());
                var listaBloqueio = await _telefoneRepository.PesquisarTelefonesNoRoteadorAsync(result, roteador);

                foreach(var item in listaBloqueio)
                {
                    result.Find(t => t.Telefone == item.Telefone).PossuiWpp = item.PossuiWpp;
                }

                baseVerificada.TelefonesVerificados.AddRange(result);

                var telefonesVerificados = baseVerificada.TelefonesVerificados.Select(t => t.Telefone).ToHashSet();
                contatos.RemoveAll(c => telefonesVerificados.Contains(c.Telefone));

                _logger.LogInformation($"No processo de limpeza interna, {baseVerificada.TelefonesVerificados.Count} foram limpos");

                if (baseVerificada.TelefonesVerificados.Any())
                {
                    await _mongoService.AdicionarDocumentoAsync(baseVerificada);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao realizar limpeza interna");
            }

        }

        private async Task LimparExternamente(List<Contato> contatos, string processId)
        {
            _logger.LogInformation("Iniciando limpeza Externa");

            try
            {
                var token = await _otimaAPI.GetToken(_credenciais);
                var lotesDeContatos = new List<ContatoRequestOtima>();

                while (contatos.Any())
                {
                    var tamanhoDoLote = Math.Min(MAX_CONTACT_REQUEST, contatos.Count);
                    lotesDeContatos.Add(new ContatoRequestOtima
                    {
                        Contacts = contatos.GetRange(0, tamanhoDoLote),
                        CallbackUrl = processId.ToString(),
                        CheckWhatsapp = true,
                    });
                    contatos.RemoveRange(0, tamanhoDoLote);
                }

                if (!lotesDeContatos.Any())
                {
                    throw new Exception();
                }

                var callbackUrl = string.Format(BASE_URL_CALLBACK, processId.ToString());
                _logger.LogInformation($"Callback URL: {callbackUrl}");
                lotesDeContatos.ForEach(lote => lote.CallbackUrl = callbackUrl);

                var tasks = lotesDeContatos.Select(async lote =>
                {
                    var json = JsonSerializer.Serialize(lote);
                    _logger.LogInformation($"Enviando lote com {lote.Contacts.Count} contatos...");
                    var idOtimaProcess = await _otimaAPI.PostContact("Bearer " + token.AccessToken, json);
                    return idOtimaProcess;
                });

                var resultados = await Task.WhenAll(tasks);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Erro ao realizar limpeza externa");
            }
        }
    }
}
