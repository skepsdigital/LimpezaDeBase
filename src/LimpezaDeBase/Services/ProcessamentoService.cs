using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using LimpezaDeBase.Configuration;
using LimpezaDeBase.Extension;
using LimpezaDeBase.Infra.Interfaces;
using LimpezaDeBase.Modelos;
using LimpezaDeBase.Modelos.Entidades;
using LimpezaDeBase.Services.Interfaces;
using System.Globalization;

namespace LimpezaDeBase.Services
{
    public class ProcessamentoService : IProcessamentoService
    {
        private readonly IAwsS3 _aws;
        private readonly IEmail _email;
        private readonly ICsvService _csvService;
        private readonly IMongoService _mongoService;
        private readonly IProcessamentoRepository _processamentoRepository;
        private readonly IRelatorioRepository _relatorioRepository;
        private readonly INotificacaoService _notificacaoService;
        private readonly ITelefoneRepository _telefoneRepository;
        private readonly ILogger<ProcessamentoService> _logger;

        public ProcessamentoService(IAwsS3 aws, IEmail email, ICsvService csvService, IMongoService mongoService, IProcessamentoRepository processamentoRepository, ILogger<ProcessamentoService> logger, INotificacaoService notificacaoService, ITelefoneRepository telefoneRepository, IRelatorioRepository relatorioRepository)
        {
            _aws = aws;
            _email = email;
            _csvService = csvService;
            _mongoService = mongoService;
            _processamentoRepository = processamentoRepository;
            _logger = logger;
            _notificacaoService = notificacaoService;
            _telefoneRepository = telefoneRepository;
            _relatorioRepository = relatorioRepository;
        }

        public async Task ProcessarCallback(List<ContatoCallbackResponse> contatoCallbackResponse, string processId)
        {
            var baseVerificada = new BaseVerificada();

            contatoCallbackResponse.NormalizarListaContatos();

            baseVerificada.ProcessId = processId;
            baseVerificada.AgenteDeLimpeza = "Otima";
            baseVerificada.TelefonesVerificados = contatoCallbackResponse.Select(c =>
            {
                var telefoneEntity = new TelefoneEntity();

                telefoneEntity.Telefone = c.Telefone.Length == 11 ? "55"+c.Telefone : c.Telefone;
                telefoneEntity.PossuiWpp = c.TemWhatsapp ?? false;

                return telefoneEntity;
            }).ToList();

            baseVerificada.TelefonesVerificados.ForEach(async t =>
            {
                t.Data = DateTime.Now;
                t.DDI = t.Telefone.Substring(0, 2);
                t.DDD = t.Telefone.Substring(2, 2);

                await _telefoneRepository.CreateAsync(t);
            });

            await _mongoService.AdicionarDocumentoAsync(baseVerificada);
        }

        public async Task ProcessarLimpezaWhatsApp()
        {
            var processamentosPendentes = await _processamentoRepository.BuscarTodosProcessamentosPendentesAsync();
            var contatosRespostaFinal = new List<TelefoneEntity>();

            _logger.LogInformation($"Foram encontrados {processamentosPendentes.Count()} processamentos");

            foreach (var item in processamentosPendentes)
            {
                var contatosSalvosNoMongo = await _mongoService.ObterDocumentoPorProcessIdAsync(item.ProcessId);

                _logger.LogInformation($"Foram encontrados para o {item.ProcessId} {contatosSalvosNoMongo.Sum(c => c.TelefonesVerificados.Count())} contatos");

                if (contatosSalvosNoMongo.Sum(c => c.TelefonesVerificados.Count()) >= item.NumeroDeContatosTotal)
                {
                    _logger.LogInformation($"Iniciando a geração do relatório para {item.ProcessId}");

                    contatosSalvosNoMongo.ForEach(contatoMongo =>
                    {
                        contatosRespostaFinal.AddRange(contatoMongo.TelefonesVerificados);
                    });

                    var relatorioCsv = new RelatorioLimpeza()
                    {
                        NumerosTotais = contatosRespostaFinal.Count(),
                        NumerosValidos = contatosRespostaFinal.Count(c => c.PossuiWpp.HasValue && c.PossuiWpp == true),
                        NumerosInvalidos = contatosRespostaFinal.Count(c => !c.PossuiWpp.HasValue || c.PossuiWpp == false),
                        Economia = (contatosRespostaFinal.Count(c => c.PossuiWpp == false) * 0.99).ToString("C", new CultureInfo("pt-BR")),
                        DDD = string.Join(',', contatosRespostaFinal.Where(c => c.Telefone.Length == 13).Select(c => c.Telefone.Substring(2, 2)).Distinct().Order())
                    };

                    var csvBytes = _csvService.GerarCsv(contatosRespostaFinal);
                    var csvRelatorioBytes = _csvService.GerarCsv<RelatorioLimpeza, RelatorioMap>(new List<RelatorioLimpeza>() { relatorioCsv });

                    var files = new List<(string fileName, byte[] fileContent)>
                    {
                        ("completo.csv", csvBytes),
                        ("relatorio.csv", csvRelatorioBytes)
                    };

                    var zipBytes = ZipFileExtension.CreateZip(files);

                    var url = await _aws.UploadZip(zipBytes, item.Contrato + item.ProcessId);
                    await _email.SendMessageAsync(item.Email, url);

                    var idNotificacoes = string.Empty;
                    if (item.EnviarNotificacao)
                    {
                        var responseNotification = await _notificacaoService.DispararNotificacoes(item.ProcessId, contatosRespostaFinal);

                        if(responseNotification is not null)
                        {
                            try
                            {
                                idNotificacoes = responseNotification.resource.id;
                            }
                            catch(Exception ex)
                            {
                            }
                        }
                    }

                    item.FoiProcessado = true;
                    await _processamentoRepository.AtualizarAsync(item);

                    var relatorio = new RelatorioEntity()
                    {
                        NumerosTotais = relatorioCsv.NumerosTotais,
                        NumerosValidos = relatorioCsv.NumerosValidos,
                        NumerosInvalidos = relatorioCsv.NumerosInvalidos,
                        LimposSkeps = contatosSalvosNoMongo.Where(c => c.AgenteDeLimpeza.Equals("Skeps")).Sum(c => c.TelefonesVerificados.Count()),
                        LimposOptOut = contatosSalvosNoMongo.Where(c => c.AgenteDeLimpeza.Equals("SkepsOptOut")).Sum(c => c.TelefonesVerificados.Count()),
                        LimposExterno = contatosSalvosNoMongo.Where(c => c.AgenteDeLimpeza.Equals("Otima")).Sum(c => c.TelefonesVerificados.Count()),
                        Contrato = item.Contrato,
                        EnviouNotificacao = item.EnviarNotificacao,
                        IdProcessamento = item.ProcessId,
                        IdNotificacoes = idNotificacoes,
                    };
                    await _relatorioRepository.AdicionarAsync(relatorio);
                }

                contatosRespostaFinal = new List<TelefoneEntity>();
            }
        }

        public async Task ProcessarLimpezaOptout(List<OptOutResultado> optoutResponse, string processId, string email)
        {
            var csvFullBytes = _csvService.GerarCsv<OptOutResultado, OptOutFullMap>(optoutResponse);
            var csvLiteBytes = _csvService.GerarCsvOptoutLite(optoutResponse.Where(o => o.NumeroEstaFormatado.Equals("Sim") && o.QuerReceberNotificao.Equals("Sim")).ToList());

            var files = new List<(string fileName, byte[] fileContent)>
            {
                ("completo.csv", csvFullBytes),
                ("basico.csv", csvLiteBytes)
            };
            var zipBytes = ZipFileExtension.CreateZip(files);

            var url = await _aws.UploadZip(zipBytes, processId);
            await _email.SendMessageAsync(email, url);

            _logger.LogInformation("Processamento finalizado.");
        }

        public async Task EnviarEmailAcompanhamento(string email, string processId)
        {
            var url = $"https://d46pjmuoog.execute-api.sa-east-1.amazonaws.com/Prod/cliente/status/{processId}";

            var textBody = $@"
                Assunto: Processamento de Limpeza Iniciado

                Olá,

                Informamos que o processamento de limpeza foi iniciado. Você pode acompanhar a etapa atual do processamento através do link abaixo:

                {url}

                Atenciosamente,
                Equipe de Suporte";

            await _email.SendMessageAsync(email, textBody);

        }
    }
}
