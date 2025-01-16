
namespace LimpezaDeBase.Services.Interfaces
{
    public interface ICatalogoService
    {
        Task CadastraCatalogoOptout(IFormFile file, string contrato, string roteador);
        Task InserirCatalogo(IFormFile file, bool eValido);
        Task InserirDadosComCopyAsync(IFormFile file);
    }
}
