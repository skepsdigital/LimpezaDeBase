using Microsoft.AspNetCore.Mvc;

namespace LimpezaDeBase.Modelos
{
    public class UploadModel
    {
        [FromForm]
        public IFormFile Arquivo { get; set; }

        [FromForm]
        public string Email { get; set; }

        [FromForm]
        public string Contrato { get; set; }

        [FromForm]
        public string Roteador { get; set; }

        [FromForm]
        public string Funcionalidade { get; set; }

        [FromForm]
        public bool EnviarNotificacao { get; set; } = false;

        [FromForm]
        public IFormFile? ArquivoExclusao { get; set; }

        [FromForm]
        public string? ChaveRoteador { get; set; }

        [FromForm]
        public string? FlowId { get; set; }

        [FromForm]
        public string? StateId { get; set; }

        [FromForm]
        public string? MasterState { get; set; }

        [FromForm]
        public string? NomeTemplate { get; set; }

        [FromForm]
        public string? NomeCampanha { get; set; }
    }
}
