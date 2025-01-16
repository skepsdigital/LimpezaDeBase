using Amazon.S3;
using Amazon.S3.Model;
using LimpezaDeBase.Infra.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LimpezaDeBase.Infra
{
    public class AwsS3 : IAwsS3
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public AwsS3(IAmazonS3 amazonS3)
        {
            _s3Client = amazonS3;
            _bucketName = "produtos.com.br";
        }

        public async Task<string> UploadImagem(IFormFile image)
        {
            // Obter o stream da imagem
            using var stream = image.OpenReadStream();

            // Determinar o tipo de conteúdo a partir da extensão do arquivo
            var contentType = image.ContentType; // O IFormFile já contém o tipo de conteúdo

            // Criar a requisição para envio ao S3
            var putRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = image.FileName, // Nome do arquivo no bucket
                InputStream = stream,
                ContentType = contentType, // Tipo de conteúdo da imagem
            };

            // Enviar para o S3
            await _s3Client.PutObjectAsync(putRequest);

            // Gerar a URL pré-assinada para acesso ao arquivo
            var url = GeneratePresignedUrl(image.FileName, TimeSpan.FromHours(24));
            return url;
        }

        public async Task<string> UploadCsv(byte[] csvBytes, string fileName)
        {
            using var stream = new MemoryStream(csvBytes);
            var putRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = fileName,
                InputStream = stream,
                ContentType = "text/csv",
            };

            await _s3Client.PutObjectAsync(putRequest);

            var url = GeneratePresignedUrl(fileName, TimeSpan.FromHours(24));
            return url;
        }

        public async Task<string> UploadZip(byte[] zipBytes, string fileName)
        {
            using var stream = new MemoryStream(zipBytes);
            var putRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = fileName,
                InputStream = stream,
                ContentType = "application/zip",
            };

            await _s3Client.PutObjectAsync(putRequest);

            var url = GeneratePresignedUrl(fileName, TimeSpan.FromHours(24));
            return url;
        }

        public async Task<byte[]> GetFileAsync(string fileName)
        {
            try
            {
                var getRequest = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = fileName
                };

                using var response = await _s3Client.GetObjectAsync(getRequest);
                using var memoryStream = new MemoryStream();
                await response.ResponseStream.CopyToAsync(memoryStream);

                return memoryStream.ToArray();
            }
            catch (AmazonS3Exception ex)
            {
                // Log erro e tratar exceção
                Console.WriteLine($"Erro ao buscar arquivo do S3: {ex.Message}");
                throw;
            }
        }

        private string GeneratePresignedUrl(string fileName, TimeSpan duration)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = fileName,
                Expires = DateTime.UtcNow.Add(duration),
            };

            return _s3Client.GetPreSignedURL(request);
        }
    }
}
