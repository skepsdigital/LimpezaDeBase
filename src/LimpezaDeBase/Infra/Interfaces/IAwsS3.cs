

namespace LimpezaDeBase.Infra.Interfaces
{
    public interface IAwsS3
    {
        Task<byte[]> GetFileAsync(string fileName);
        Task<string> UploadCsv(byte[] csvBytes, string fileName);
        Task<string> UploadImagem(IFormFile image);
        Task<string> UploadZip(byte[] zipBytes, string fileName);
    }
}
