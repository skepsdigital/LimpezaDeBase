using LimpezaDeBase.Modelos;

namespace LimpezaDeBase.Limpeza.Interfaces
{
    public interface ILimpezaService
    {
        Task<bool> ExecutarLimpeza(UploadModel upload);
    }
}
