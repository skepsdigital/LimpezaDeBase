using Dapper;
using LimpezaDeBase.Infra.Interfaces;
using LimpezaDeBase.Modelos.Entidades;
using Npgsql;
using System.Data;

namespace LimpezaDeBase.Infra.Repository
{
    public class TelefoneRepository : ITelefoneRepository
    {
        private readonly string _connectionString;

        public TelefoneRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        private IDbConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        public async Task<List<TelefoneEntity>> PesquisarTelefonesNoRoteadorAsync(List<TelefoneEntity> numeros, string roteador)
        {
            try
            {
                var telefonesEncontrados = new List<TelefoneEntity>();

                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                // Criar tabela temporária
                var criarTabelaTemporariaQuery = @"
                CREATE TEMP TABLE TempNumeros (
                    Telefone VARCHAR(45)
                )";

                var dropTabelaTemporariaQuery = @"DROP TABLE IF EXISTS TempNumeros";

                using (var cmd = new NpgsqlCommand(criarTabelaTemporariaQuery, connection))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                // Usar COPY para inserir os números na tabela temporária
                using (var writer = connection.BeginBinaryImport("COPY TempNumeros (Telefone) FROM STDIN (FORMAT BINARY)"))
                {
                    foreach (var numero in numeros)
                    {
                        writer.StartRow();
                        writer.Write(numero.Telefone, NpgsqlTypes.NpgsqlDbType.Varchar);
                    }
                    writer.Complete();
                }

                // Pesquisar números na tabela `Roteador`
                var query = @"
                    SELECT r.*
                    FROM Roteador r
                    INNER JOIN TempNumeros tn
                    ON r.TelefoneID = tn.Telefone
                    WHERE r.Roteador = @Roteador";

                using (var cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Roteador", NpgsqlTypes.NpgsqlDbType.Varchar, roteador);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var roteadorEntity = new TelefoneEntity
                            {
                                Telefone = reader.GetString(reader.GetOrdinal("telefoneid")),
                                PossuiWpp = reader.GetString(reader.GetOrdinal("existebloqueio")) != "true",
                            };
                            telefonesEncontrados.Add(roteadorEntity);
                        }
                    }
                }

                // Remover tabela temporária
                using (var cmd = new NpgsqlCommand(dropTabelaTemporariaQuery, connection))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                return telefonesEncontrados;
            }
            catch (Exception ex)
            {
                // Logar o erro para depuração, se necessário
                Console.WriteLine(ex.Message);
                return new List<TelefoneEntity>();
            }
        }


        public async Task<List<TelefoneEntity>> PesquisarNumerosAsync(List<string> numeros)
        {
            try
            {
                var telefonesEncontrados = new List<TelefoneEntity>();

                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                // Criar tabela temporária
                var criarTabelaTemporariaQuery = @"
                        CREATE TEMP TABLE TempNumeros (
                            Telefone VARCHAR(45)
                        )";

                var dropTabelaTemporariaQuery = @"DROP TABLE IF EXISTS TempNumeros";

                using (var cmd = new NpgsqlCommand(criarTabelaTemporariaQuery, connection))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                // Usar COPY para inserir os números na tabela temporária
                using (var writer = connection.BeginBinaryImport("COPY TempNumeros (Telefone) FROM STDIN (FORMAT BINARY)"))
                {
                    foreach (var numero in numeros)
                    {
                        writer.StartRow();
                        writer.Write(numero, NpgsqlTypes.NpgsqlDbType.Varchar);
                    }
                    writer.Complete();
                }

                // Pesquisar números na tabela `Telefone`
                var query = @"
                            SELECT t.*
                            FROM Telefone t
                            INNER JOIN TempNumeros tn
                            ON t.Telefone = tn.Telefone;";

                using (var cmd = new NpgsqlCommand(query, connection))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var telefone = new TelefoneEntity
                        {
                            Telefone = reader.GetString(reader.GetOrdinal("Telefone")),
                            PossuiWpp = reader.GetBoolean(reader.GetOrdinal("PossuiWpp")),
                        };
                        telefonesEncontrados.Add(telefone);
                    }
                }

                using (var cmd = new NpgsqlCommand(dropTabelaTemporariaQuery, connection))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                return telefonesEncontrados;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        public async Task<int> CreateAsync(TelefoneEntity telefone)
        {
            const string query = @"
                INSERT INTO Telefone (Telefone, DDI, DDD, PossuiWpp, Data)
                VALUES (@Telefone, @DDI, @DDD, @PossuiWpp, @Data)
                RETURNING ID;
            ";

            using var connection = CreateConnection();
            return await connection.ExecuteScalarAsync<int>(query, telefone);
        }
        public async Task<int> CreateManyAsync(IEnumerable<TelefoneEntity> telefones)
        {
            const string query = @"
                INSERT INTO Telefone (Telefone, DDI, DDD, PossuiWpp, Data)
                VALUES (@Telefone, @DDI, @DDD, @PossuiWpp, @Data);
            ";

            using var connection = CreateConnection();

            var parameters = telefones.Select(telefone => new
            {
                Telefone = telefone.Telefone,
                DDI = telefone.Telefone.Substring(0, 2),
                DDD = telefone.Telefone.Substring(2, 2),
                PossuiWpp = telefone.PossuiWpp,
                Data = DateTime.Now
            });

            // Executa todas as inserções em uma única operação
            return await connection.ExecuteAsync(query, parameters);
        }
        public async Task<TelefoneEntity?> GetByIdAsync(int id)
        {
            const string query = "SELECT * FROM Telefone WHERE ID = @ID;";

            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<TelefoneEntity>(query, new { ID = id });
        }

        public async Task<IEnumerable<TelefoneEntity>> GetAllAsync()
        {
            const string query = "SELECT * FROM Telefone;";

            using var connection = CreateConnection();
            return await connection.QueryAsync<TelefoneEntity>(query);
        }

        public async Task<bool> UpdateAsync(TelefoneEntity telefone)
        {
            const string query = @"
                UPDATE Telefone
                SET Telefone = @Telefone,
                    DDI = @DDI,
                    DDD = @DDD,
                    PossuiWpp = @PossuiWpp,
                    NomeProprietario = @NomeProprietario,
                    Data = @Data
                WHERE ID = @ID;
            ";

            using var connection = CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(query, telefone);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string query = "DELETE FROM Telefone WHERE ID = @ID;";

            using var connection = CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(query, new { ID = id });
            return rowsAffected > 0;
        }
    }
}
