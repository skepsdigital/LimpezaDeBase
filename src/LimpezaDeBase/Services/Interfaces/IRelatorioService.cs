
namespace LimpezaDeBase.Services.Interfaces
{
    public interface IRelatorioService
    {
        Task<Dictionary<string, int>> ObterStatusCampanhaNotificacao(string idProcessamento);
    }
}
