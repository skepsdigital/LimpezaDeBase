using LimpezaDeBase.Modelos.Entidades;

namespace LimpezaDeBase.Infra.Interfaces
{
    public interface IRelatorioRepository
    {
        Task<int> AdicionarAsync(RelatorioEntity relatorio);
        Task AtualizarAsync(RelatorioEntity relatorio);
        Task<IEnumerable<RelatorioEntity>> BuscarPorProcessamentoAsync(string idProcessamento);
        Task<IEnumerable<RelatorioEntity>> BuscarTodosAsync();
        Task DeletarPorIdAsync(int id);
    }
}
