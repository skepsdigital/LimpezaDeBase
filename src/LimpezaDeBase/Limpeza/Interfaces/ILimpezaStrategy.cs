using LimpezaDeBase.Modelos;

namespace LimpezaDeBase.Limpeza.Interfaces
{
    public interface ILimpezaStrategy
    {
        Task Validar(List<Contato> contatos, string processId, string contrato, string roteador, string email, int numeroDeContatos, bool enviarNotitificacao, List<Contato>? contatoExclusao = null);
    }
}
