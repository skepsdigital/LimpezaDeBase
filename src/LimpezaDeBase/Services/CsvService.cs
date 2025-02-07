using CsvHelper;
using CsvHelper.Configuration;
using LimpezaDeBase.Configuration;
using LimpezaDeBase.Modelos;
using LimpezaDeBase.Modelos.Entidades;
using LimpezaDeBase.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace LimpezaDeBase.Services
{
    public class CsvService : ICsvService
    {
        public CsvService()
        {
            
        }

        public async Task<List<Contato>> LerArquivoEPreecherLista(IFormFile file)
        {
            try
            {
                var contatos = new List<Contato>();
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        HasHeaderRecord = true,
                        Delimiter = ",",
                        MissingFieldFound = null
                    };

                    using (var reader = new StreamReader(stream))
                    using (var csv = new CsvReader(reader, csvConfig))
                    {
                        csv.Context.RegisterClassMap<ContatoMap>();

                        contatos = csv.GetRecords<Contato>().ToList();
                    }
                }

                return contatos;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public byte[] GerarCsv(List<TelefoneEntity> response)
        {
            using var memoryStream = new MemoryStream();
            using var streamWriter = new StreamWriter(memoryStream);
            using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);

            // Registrar o mapeamento personalizado
            csvWriter.Context.RegisterClassMap<TelefoneEntityMap>();

            csvWriter.WriteRecords(response);
            streamWriter.Flush();

            return memoryStream.ToArray();
        }

        public byte[] GerarCsv(List<OptOutResultado> response)
        {
            using var memoryStream = new MemoryStream();
            using var streamWriter = new StreamWriter(memoryStream);
            using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);

            // Registrar o mapeamento personalizado
            csvWriter.Context.RegisterClassMap<OptOutFullMap>();

            csvWriter.WriteRecords(response);
            streamWriter.Flush();

            return memoryStream.ToArray();
        }

        public byte[] GerarCsvOptoutLite(List<OptOutResultado> response)
        {
            using var memoryStream = new MemoryStream();
            using var streamWriter = new StreamWriter(memoryStream);
            using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);

            // Registrar o mapeamento personalizado
            csvWriter.Context.RegisterClassMap(new OptOutLiteMap(response.SelectMany(r => r.Extras.Keys).Distinct()));

            csvWriter.WriteRecords(response);
            streamWriter.Flush();

            return memoryStream.ToArray();
        }

        public byte[] GerarCsv<T,R>(List<T> response) where R : ClassMap<T>, new()
        {
            using var memoryStream = new MemoryStream();
            using var streamWriter = new StreamWriter(memoryStream);
            using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);

            // Registrar o mapeamento personalizado
            csvWriter.Context.RegisterClassMap<R>();

            csvWriter.WriteRecords(response);
            streamWriter.Flush();

            return memoryStream.ToArray();
        }

        public async Task<List<dynamic>> LerCsvComoDinamico(byte[] csvBytes)
        {
            try
            {
                using var memoryStream = new MemoryStream(csvBytes);
                using var streamReader = new StreamReader(memoryStream);
                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    Delimiter = ",",
                    MissingFieldFound = null
                };

                using var csvReader = new CsvReader(streamReader, csvConfig);

                // Lê os registros como objetos dinâmicos
                var records = new List<dynamic>();
                await foreach (var record in csvReader.GetRecordsAsync<dynamic>())
                {
                    records.Add(record);
                }

                return records;
            }
            catch (Exception ex)
            {
                // Log de erro, se necessário
                return null;
            }
        }
    }
}
