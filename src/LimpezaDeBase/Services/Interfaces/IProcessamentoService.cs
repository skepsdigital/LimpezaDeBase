using LimpezaDeBase.Modelos;

namespace LimpezaDeBase.Services.Interfaces
{
    public interface IProcessamentoService
    {
        Task EnviarEmailAcompanhamento(string email, string processId);
        Task ProcessarCallback(List<ContatoCallbackResponse> contatoCallbackResponse, string processId);
        Task ProcessarLimpezaOptout(List<OptOutResultado> optoutResponse, string processId, string email);
        Task ProcessarLimpezaWhatsApp();
    }
}
