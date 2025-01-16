using LimpezaDeBase.Modelos;
using LimpezaDeBase.Services.Interfaces;
using LimpezaDeBase.Limpeza.Interfaces;
using LimpezaDeBase.Extension;
using LimpezaDeBase.Infra.Interfaces;
using LimpezaDeBase.Modelos.Mongo;

namespace LimpezaDeBase.Limpeza
{
    public class LimpezaService : ILimpezaService
    {
        private readonly IMongoService _mongoService;
        private readonly ICsvService _csvService;
        private readonly IAwsS3 _aws;
        private readonly ILogger<LimpezaService> _logger;
        private readonly LimpezaFactory _limpezaFactory;

        public LimpezaService(IMongoService mongoService, ICsvService csvService, LimpezaFactory limpezaFactory, ILogger<LimpezaService> logger, IAwsS3 aws)
        {
            _mongoService = mongoService;
            _csvService = csvService;
            _logger = logger;
            _limpezaFactory = limpezaFactory;
            _aws = aws;
        }

        public async Task<bool> ExecutarLimpeza(UploadModel upload)
        {
            var processId = Guid.NewGuid();
            var contatos = new List<Contato>();
            var strategy = _limpezaFactory.GetStrategy(upload.Funcionalidade);

            _logger.LogInformation("GUID -> " + processId.ToString());

            try
            {
                _logger.LogInformation($"Iniciando o processo de limpeza - {upload.Funcionalidade}");

                contatos = await _csvService.LerArquivoEPreecherLista(upload.Arquivo);
                
                var contatosExclusao = new List<Contato>();
                if(upload.ArquivoExclusao is not null)
                {
                    contatosExclusao = await _csvService.LerArquivoEPreecherLista(upload.ArquivoExclusao);
                    contatosExclusao.RemoveAt(0);
                    contatosExclusao.NormalizarListaContatos();
                }

                var cliente = (await _mongoService.ObterTodosClientesAsync()).FirstOrDefault(c => c.Contrato.Equals($"{upload.Contrato}"));

                if (cliente is null || contatos is null || contatos.Count() >= cliente.Creditos)
                    throw new Exception("Cliente invalido, lista sem contatos ou cliente sem crédito");

                if(cliente.Funcionalidades is not null && cliente.Funcionalidades.First() != (upload.Funcionalidade))
                    throw new Exception("Cliente não pode usar essa funcionalidade");

                _logger.LogInformation($"Cliente encontrado {cliente.Nome} - {cliente.Contrato}");

                var numeroDeContatos = contatos.Count();
                _logger.LogInformation($"{numeroDeContatos} foram encontrados");

                contatos.NormalizarListaContatos();

                _logger.LogInformation($"Processo -> {processId.ToString()}. Enviar notificação:{upload.EnviarNotificacao}");
                if(upload.EnviarNotificacao)
                {
                    await _mongoService.AdicionarNotificacaoDadosAsync(new NotificacaoDadosDB()
                    {
                        Contrato = upload.Contrato,
                        FlowId = upload.FlowId,
                        KeyRoteador = upload.ChaveRoteador,
                        StateId = upload.StateId,
                        MasterState = upload.MasterState,
                        Template = upload.NomeTemplate,
                        NomeCampanha = upload.NomeCampanha,
                        ProcessId = processId.ToString()
                    });
                    var fileBytes = await ConvertIFormFileToBytes(upload.Arquivo);
                    await _aws.UploadCsv(fileBytes, $"fileOriginal-{processId}");
                }

                await strategy.Validar(contatos, processId.ToString(), upload.Contrato, upload.Roteador, upload.Email, numeroDeContatos, upload.EnviarNotificacao, contatosExclusao.Any() ? contatosExclusao : null);

                _logger.LogInformation("Processamento finalizado.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro {0}", ex.Message);
                throw;
            }
        }

        public async Task<byte[]> ConvertIFormFileToBytes(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
