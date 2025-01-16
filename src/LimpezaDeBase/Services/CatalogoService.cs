using CsvHelper.Configuration;
using CsvHelper;
using LimpezaDeBase.Configuration;
using LimpezaDeBase.Modelos;
using System.Globalization;
using LimpezaDeBase.Services.Interfaces;
using LimpezaDeBase.Modelos.Mongo;
using Npgsql;

namespace LimpezaDeBase.Services
{
    public class CatalogoService : ICatalogoService
    {
        private readonly IMongoService _mongoService;
        private readonly ILogger<CatalogoService> _logger;

        public CatalogoService(IMongoService mongoService, ILogger<CatalogoService> logger)
        {
            _mongoService = mongoService;
            _logger = logger;
        }
        public async Task<List<string>> LerArquivoParaListaAsync(IFormFile file)
        {
            var linhas = new List<string>();

            // Verifica se o arquivo é nulo
            if (file == null || file.Length == 0)
            {
                return linhas;
            }

            // Usando o StreamReader para ler o arquivo
            using (var stream = new StreamReader(file.OpenReadStream()))
            {
                while (!stream.EndOfStream)
                {
                    // Lê cada linha do arquivo
                    var linha = await stream.ReadLineAsync();
                    if (linha != null)
                    {
                        // Adiciona a linha à lista
                        linhas.Add(linha);
                    }
                }
            }

            return linhas.Distinct().ToList();
        }

        public async Task InserirDadosComCopyAsync(IFormFile file)
        {
            var dropTabelaTemporariaQuery = @"DROP TABLE IF EXISTS TempNumeros";

            var telefonesTotal = await LerArquivoParaListaAsync(file);

            while(telefonesTotal.Any())
            {
                var telefones = telefonesTotal.Skip(1).Take(1000).ToList();

                using var connection = new NpgsqlConnection("Host=skepsdata.postgres.database.azure.com;Port=5432;Database=disparador;Username=christian;Password=skeps@123;Timeout=300;CommandTimeout=300;");
                await connection.OpenAsync();

                // Criar uma tabela temporária
                await using (var createTempTableCommand = new NpgsqlCommand(@"
                CREATE TEMP TABLE temp_telefone (
                    Telefone VARCHAR PRIMARY KEY,
                    DDI VARCHAR,
                    DDD VARCHAR,
                    PossuiWpp BOOLEAN,
                    Data TIMESTAMP
                );
            ", connection))
                {
                    await createTempTableCommand.ExecuteNonQueryAsync();
                }

                // Inserir dados na tabela temporária com COPY
                using (var writer = connection.BeginBinaryImport(@"
                COPY temp_telefone (Telefone, DDI, DDD, PossuiWpp, Data)
                FROM STDIN (FORMAT BINARY)
            "))
                {
                    foreach (var contato in telefones)
                    {
                        writer.StartRow();
                        writer.Write(contato.Split(',').First(), NpgsqlTypes.NpgsqlDbType.Varchar);
                        writer.Write(contato.Substring(0, 2), NpgsqlTypes.NpgsqlDbType.Varchar);
                        writer.Write(contato.Substring(2, 2), NpgsqlTypes.NpgsqlDbType.Varchar);
                        writer.Write(contato.Split(',').Last() == "true", NpgsqlTypes.NpgsqlDbType.Boolean);
                        writer.Write(DateTime.Now, NpgsqlTypes.NpgsqlDbType.Timestamp);
                    }

                    writer.Complete();
                }

                // Mover dados da tabela temporária para a tabela principal, tratando duplicados
                await using (var insertCommand = new NpgsqlCommand(@"
                INSERT INTO Telefone (Telefone, DDI, DDD, PossuiWpp, Data)
                SELECT Telefone, DDI, DDD, PossuiWpp, Data
                FROM temp_telefone
                ON CONFLICT (Telefone) DO UPDATE
                SET 
                    DDI = EXCLUDED.DDI,
                    DDD = EXCLUDED.DDD,
                    PossuiWpp = EXCLUDED.PossuiWpp,
                    Data = EXCLUDED.Data;
            ", connection))
                {
                    await insertCommand.ExecuteNonQueryAsync();
                }

                using (var cmd = new NpgsqlCommand(dropTabelaTemporariaQuery, connection))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                telefonesTotal.RemoveRange(1, 1000);
            }
        }


        public async Task InserirCatalogo(IFormFile file, bool eValido)
        {
            //var contatos = new List<Contato>();
            //var catalogo = new CatalgoContatoDB();
            //using (var stream = new MemoryStream())
            //{
            //    await file.CopyToAsync(stream);
            //    stream.Position = 0;

            //    var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            //    {
            //        HasHeaderRecord = false,
            //        Delimiter = ",",
            //        MissingFieldFound = null
            //    };

            //    using (var reader = new StreamReader(stream))
            //    using (var csv = new CsvReader(reader, csvConfig))
            //    {
            //        csv.Context.RegisterClassMap<ContatoMap>();

            //        contatos = csv.GetRecords<Contato>().ToList().DistinctBy(c => c.Telefone).ToList();
            //    }
            //}
            //contatos.RemoveAt(0);

            var catalogos = await _mongoService.ObterCatalogoDBAsync();

            foreach (var item in catalogos)
            {
                item.Numeros = item.Numeros.ToDictionary(
                    kvp => kvp.Key.Length == 11 ? "55"+kvp.Key : kvp.Key,
                    kvp => kvp.Value
                );
                item.Id = MongoDB.Bson.ObjectId.GenerateNewId();
            }

            await _mongoService.InserirCatalogosDBAsync(catalogos);
        }

        public async Task CadastraCatalogoOptout(IFormFile file, string contrato, string roteador)
        {
            var contatos = new List<Contato>();
            var catalogo = new CatalgoContatoDB();
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = false,
                    Delimiter = ",",
                    MissingFieldFound = null
                };

                using (var reader = new StreamReader(stream))
                using (var csv = new CsvReader(reader, csvConfig))
                {
                    csv.Context.RegisterClassMap<ContatoMap>();

                    contatos = csv.GetRecords<Contato>().ToList().DistinctBy(c => c.Telefone).ToList();
                }
            }
            contatos.RemoveAt(0);

            var optouts = new OptOutDB();
            optouts.Telefone =
            [
                .. contatos.Select(c =>
                {
                    return c.Telefone;
                }).ToList(),
            ];

            optouts.Contrato = contrato;
            optouts.Roteador = roteador;

            await _mongoService.InserirOptOutAsync(optouts);
        }
    }
}
