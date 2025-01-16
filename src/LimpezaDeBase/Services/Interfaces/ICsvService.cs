using CsvHelper.Configuration;
using LimpezaDeBase.Modelos;
using LimpezaDeBase.Modelos.Entidades;

namespace LimpezaDeBase.Services.Interfaces
{
    public interface ICsvService
    {
        byte[] GerarCsv(List<TelefoneEntity> response);
        byte[] GerarCsv(List<OptOutResultado> response);
        byte[] GerarCsv<T, R>(List<T> response) where R : ClassMap<T>, new();
        byte[] GerarCsvOptoutLite(List<OptOutResultado> response);
        Task<List<Contato>> LerArquivoEPreecherLista(IFormFile file);
        Task<List<dynamic>> LerCsvComoDinamico(byte[] csvBytes);
    }
}
