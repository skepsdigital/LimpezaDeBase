using LimpezaDeBase.Infra.Interfaces;
using LimpezaDeBase.Limpeza.Interfaces;
using LimpezaDeBase.Modelos;
using LimpezaDeBase.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LimpezaDeBase.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LimparController : ControllerBase
    {
        private readonly ILogger<LimparController> _logger;
        private readonly ILimpezaService _limpezaService;
        private readonly IProcessamentoService _processamentoService;
        private readonly IMongoService _mongoService;
        private readonly IEmail _email;

        public LimparController(ILogger<LimparController> logger, ILimpezaService limpezaService, IMongoService mongoService, IEmail email, IProcessamentoService processamentoService)
        {
            _logger = logger;
            _limpezaService = limpezaService;
            _mongoService = mongoService;
            _email = email;
            _processamentoService = processamentoService;
        }

        /// <summary>
        /// Receives a CSV file and an email address for processing.
        /// </summary>
        /// <param name="file">CSV file to be uploaded.</param>
        /// <param name="email">Email address for notification.</param>
        /// <returns>Response indicating whether the file and email were received successfully.</returns>
        [HttpPost("upload-csv")]
        public async Task<IActionResult> UploadCsvAndEmail([FromForm] UploadModel upload)
        {
            // Verifica se o arquivo foi enviado
            if (upload.Arquivo == null || upload.Arquivo.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            if (string.IsNullOrWhiteSpace(upload.Email) || string.IsNullOrWhiteSpace(upload.Contrato) || string.IsNullOrWhiteSpace(upload.Roteador) || string.IsNullOrWhiteSpace(upload.Funcionalidade))
            {
                return BadRequest("Email address is required.");
            }

            try
            {
                var result = await _limpezaService.ExecutarLimpeza(upload);

                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPost("callback/{processId}")]
        public async Task<IActionResult> Callback([FromBody]List<ContatoCallbackResponse> contatoCallbackResponse, [FromRoute]string processId)
        {
            _logger.LogInformation($"Recebendo callback{contatoCallbackResponse.Count()}");

            await _processamentoService.ProcessarCallback(contatoCallbackResponse, processId);

            _logger.LogInformation("Finalizando callback");

            return Ok("Documento adicionado com sucesso");
        }

        [HttpGet("processarLimpeza")]
        public async Task<IActionResult> ProcessarLimpeza()
        {
            
            
           _logger.LogInformation("Iniciando tentativa de processar resultados");
            await _processamentoService.ProcessarLimpezaWhatsApp();

            return Ok("Documento adicionado com sucesso");
        }
    }
}
