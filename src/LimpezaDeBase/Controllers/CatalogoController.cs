using LimpezaDeBase.Infra.Interfaces;
using LimpezaDeBase.Modelos;
using LimpezaDeBase.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LimpezaDeBase.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CatalogoController : ControllerBase
    {
        private readonly ILogger<CatalogoController> _logger;
        private readonly IBlipContatoService _blipContatoService;
        private readonly ICatalogoService _catalogoService;
        private readonly IOptOutService _optOutService;

        public CatalogoController(ILogger<CatalogoController> logger, ICatalogoService catalogoService, IOptOutService optOutService)
        {
            _logger = logger;
            _catalogoService = catalogoService;
            _optOutService = optOutService;
        }

        [HttpPost("preencher")]
        public async Task<IActionResult> Preencher(IFormFile file)
        {
            await _catalogoService.InserirDadosComCopyAsync(file);
            return Ok("Documento adicionado com sucesso");
        }

        [HttpPost("preencherCatalogoInterno")]
        public async Task<IActionResult> PreencherCatalogoInterno([FromForm] IFormFile file)
        {
            await _catalogoService.InserirCatalogo(file, true);
            return Ok("Documento adicionado com sucesso");
        }

        [HttpPost("preencherCatalogoOptout")]
        public async Task<IActionResult> PreencherCatalogo([FromForm] IFormFile file, string contrato, string roteador)
        {
            await _catalogoService.CadastraCatalogoOptout(file, contrato, roteador);
            return Ok("Documento adicionado com sucesso");
        }

        [HttpGet("InserirOptOut")]
        public async Task<IActionResult> InserirOptOut(string telefone, string contrato, string roteador)
        {
            var result = await _optOutService.InserirOptOut(telefone, contrato, roteador);

            return result ? Ok() : BadRequest();
        }

        [HttpGet("RemoverOptOut")]
        public async Task<IActionResult> RemoverOptOut(string telefone, string contrato, string roteador)
        {
            var result = await _optOutService.RemoverOptOut(telefone, contrato, roteador);

            return result ? Ok() : BadRequest();
        }
    }
}
