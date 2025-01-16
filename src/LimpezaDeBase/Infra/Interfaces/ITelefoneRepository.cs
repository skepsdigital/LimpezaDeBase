using LimpezaDeBase.Modelos.Entidades;

namespace LimpezaDeBase.Infra.Interfaces
{
    public interface ITelefoneRepository
    {
        Task<int> CreateAsync(TelefoneEntity telefone);
        Task<TelefoneEntity?> GetByIdAsync(int id);
        Task<IEnumerable<TelefoneEntity>> GetAllAsync();
        Task<bool> UpdateAsync(TelefoneEntity telefone);
        Task<bool> DeleteAsync(int id);
        Task<List<TelefoneEntity>> PesquisarNumerosAsync(List<string> numeros);
        Task<List<TelefoneEntity>> PesquisarTelefonesNoRoteadorAsync(List<TelefoneEntity> numeros, string roteador);
    }
}
