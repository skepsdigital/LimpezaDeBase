using LimpezaDeBase.Modelos.Entidades;

namespace LimpezaDeBase.Services.Interfaces
{
    public interface INotificacaoService
    {
        Task<dynamic> DispararNotificacoes(string processId, List<TelefoneEntity> telefonesProcessados);
    }
}