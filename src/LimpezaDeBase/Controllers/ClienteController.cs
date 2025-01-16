using LimpezaDeBase.Infra.Interfaces;
using LimpezaDeBase.Modelos;
using LimpezaDeBase.Modelos.Mongo;
using LimpezaDeBase.Services;
using LimpezaDeBase.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LimpezaDeBase.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClienteController : ControllerBase
    {
        private readonly ILogger<ClienteController> _logger;
        private readonly IMongoService _mongoService;
        private readonly IRelatorioService _relatorioService;
        private readonly IEmail _email;
        private readonly IAwsS3 _awsS3;

        public ClienteController(ILogger<ClienteController> logger, IMongoService mongoService, IEmail email, IRelatorioService relatorioService)
        {
            _logger = logger;
            _mongoService = mongoService;
            _email = email;
            _relatorioService = relatorioService;
        }

        [HttpPost("criar")]
        public async Task<IActionResult> CriarCliente(ClienteDB cliente)
        {
            await _mongoService.AdicionarClienteAsync(cliente);

            return Ok("Cliente adicionando com sucesso");
        }

        [HttpGet("obtertodos")]
        public async Task<IActionResult> ObterTodosClientes()
        {
            var clientes = await _mongoService.ObterTodosClientesAsync();

            return Ok(clientes);
        }


        [HttpGet("status/{processId}")]
        public async Task<IActionResult> Callback([FromRoute] string processId)
        {
            var result = await _relatorioService.ObterStatusCampanhaNotificacao(processId);

            if(result == null)
            {
                return Ok("Ainda estamos realizando a limpeza, volte daqui alguns minutos para mais informações");
            }

            return Ok(result);
        }

        [HttpPost("upload-img")]
        public async Task<IActionResult> UploadImg([FromForm] IFormFile upload)
        {
            var link = await _awsS3.UploadImagem(upload);

            return Ok(link);
        }
    }
}
