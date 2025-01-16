using LimpezaDeBase.Infra.Interfaces;
using LimpezaDeBase.Limpeza.Interfaces;
using LimpezaDeBase.Modelos;
using LimpezaDeBase.Modelos.Entidades;
using LimpezaDeBase.Services;
using LimpezaDeBase.Services.Interfaces;
using System.Text.RegularExpressions;

namespace LimpezaDeBase.Limpeza.Strategy
{
    public class LimparOptOutStrategy : ILimpezaStrategy
    {
        private readonly IMongoService _mongoService;
        private readonly IProcessamentoRepository _processamentoRepository;
        private readonly IProcessamentoService _processamentoService;
        private readonly ILogger<LimparOptOutStrategy> _logger;
        private static readonly Regex telefoneRegex = new Regex(@"^\d{4}\d{9}$");

        public LimparOptOutStrategy(ILogger<LimparOptOutStrategy> logger, IMongoService mongoService, IProcessamentoRepository processamentoRepository, IProcessamentoService processamentoService)
        {
            _mongoService = mongoService;
            _processamentoRepository = processamentoRepository;
            _logger = logger;
            _processamentoService = processamentoService;
        }

        public async Task Validar(List<Contato> contatos, string processId, string contrato, string roteador, string email, int numeroDeContatos, bool enviarNotificacao, List<Contato>? contatoExclusao = null)
        {
            var optouts = (await _mongoService.ObterOptOutPorContrato(contrato)).FirstOrDefault(o => o.Roteador == roteador);
            var optoutResponse = new List<OptOutResultado>();

            if(optouts is null)
            {
                throw new Exception("Não foram encontrado dados para esse contrato e roteador");
            }

            _logger.LogInformation($"{optouts.Telefone.Count} de optouts foram encontrados - {processId.ToString()}");

            if(contatoExclusao is not null)
            {
                contatos = contatos.Except(contatoExclusao, new ContatoComparer()).ToList();
            }

            contatos = contatos.Distinct(new ContatoComparer()).ToList();

            foreach (var contatoUnico in contatos)
            {
                optoutResponse.Add(new OptOutResultado()
                {
                    Telefone = contatoUnico.Telefone,
                    Extras = contatoUnico.Extras,
                    NumeroEstaFormatado = telefoneRegex.IsMatch(contatoUnico.Telefone) ? "Sim" : "Não",
                    QuerReceberNotificao = optouts.Telefone.Contains(contatoUnico.Telefone) ? "Não" : "Sim",
                });
            }

            optoutResponse.ForEach(op =>
            {
                if(op.NumeroEstaFormatado == "Não")
                {
                    op.QuerReceberNotificao = "Não";
                }
            });

            await _processamentoService.ProcessarLimpezaOptout(optoutResponse, processId, email);

            await _processamentoRepository.AdicionarAsync(new ProcessamentoEntity
            {
                Contrato = contrato,
                Roteador = roteador,
                Data = DateTime.Now,
                Email = email,
                FoiProcessado = true,
                NumeroDeContatosTotal = numeroDeContatos,
                ProcessId = processId.ToString(),
                EnviarNotificacao = false,
                Funcionalidade = "blocklist"
            });
        }
    }
}
