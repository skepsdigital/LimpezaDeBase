using LimpezaDeBase.Modelos.Entidades;

namespace LimpezaDeBase.Infra.Interfaces
{
    public interface IProcessamentoRepository
    {
        Task<int> AdicionarAsync(ProcessamentoEntity processamento);
        Task AtualizarAsync(ProcessamentoEntity processamento);
        Task<IEnumerable<ProcessamentoEntity>> BuscarTodosProcessamentosPendentesAsync();
    }
}
