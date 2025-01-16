using LimpezaDeBase.Modelos.BlipContatos;

namespace LimpezaDeBase.Infra.Interfaces
{
    public interface IBlip
    {
        void CriarHttpClient(string contractId, string token);
        Task<BlipContatoResponse> GetContacts(int skip, int take);
    }
}
