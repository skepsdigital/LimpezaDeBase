

namespace LimpezaDeBase.Services.Interfaces
{
    public interface IOptOutService
    {
        Task<bool> InserirOptOut(string telefone, string contrato, string roteador);
        Task<bool> RemoverOptOut(string telefone, string contrato, string roteador);
    }
}
