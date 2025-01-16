using Dapper;
using LimpezaDeBase.Infra.Interfaces;
using LimpezaDeBase.Modelos.Entidades;
using Npgsql;

namespace LimpezaDeBase.Infra.Repository
{
    public class RelatorioRepository : IRelatorioRepository
    {
        private readonly string _connectionString;

        public RelatorioRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> AdicionarAsync(RelatorioEntity relatorio)
        {
            try
            {
                const string query = @"
                INSERT INTO Relatorio 
                    (IdProcessamento, NumerosTotais, NumerosValidos, NumerosInvalidos, 
                     LimposSkeps, LimposExterno, LimposOptOut, EnviouNotificacao, Contrato, IdNotificacoes)
                VALUES 
                    (@IdProcessamento, @NumerosTotais, @NumerosValidos, @NumerosInvalidos, 
                     @LimposSkeps, @LimposExterno, @LimposOptOut, @EnviouNotificacao, @Contrato, @IdNotificacoes)
                RETURNING ID;";

                using var connection = new NpgsqlConnection(_connectionString);
                return await connection.ExecuteScalarAsync<int>(query, relatorio);
            }
            catch (Exception ex)
            {
                // Lidar com o erro conforme necessário (logar, lançar exceção personalizada, etc.)
                return 0;
            }
        }

        public async Task AtualizarAsync(RelatorioEntity relatorio)
        {
            const string query = @"
                UPDATE Relatorio
                SET
                    NumerosTotais = @NumerosTotais,
                    NumerosValidos = @NumerosValidos,
                    NumerosInvalidos = @NumerosInvalidos,
                    LimposSkeps = @LimposSkeps,
                    LimposExterno = @LimposExterno,
                    LimposOptOut = @LimposOptOut,
                    EnviouNotificacao = @EnviouNotificacao,
                    Contrato = @Contrato,
                    IdNotificacoes = @IdNotificacoes
                WHERE ID = @Id;";

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.ExecuteAsync(query, relatorio);
        }

        public async Task<IEnumerable<RelatorioEntity>> BuscarPorProcessamentoAsync(string idProcessamento)
        {
            const string query = @"
                SELECT
                    Id,
                    IdProcessamento,
                    NumerosTotais,
                    NumerosValidos,
                    NumerosInvalidos,
                    LimposSkeps,
                    LimposExterno,
                    LimposOptOut,
                    EnviouNotificacao,
                    Contrato,
                    IdNotificacoes
                FROM Relatorio
                WHERE IdProcessamento = @IdProcessamento;";

            using var connection = new NpgsqlConnection(_connectionString);
            return await connection.QueryAsync<RelatorioEntity>(query, new { IdProcessamento = idProcessamento });
        }

        public async Task<IEnumerable<RelatorioEntity>> BuscarTodosAsync()
        {
            const string query = @"
                SELECT
                    Id,
                    IdProcessamento,
                    NumerosTotais,
                    NumerosValidos,
                    NumerosInvalidos,
                    LimposSkeps,
                    LimposExterno,
                    LimposOptOut,
                    EnviouNotificacao,
                    Contrato,
                    IdNotificacoes
                FROM Relatorio
                ORDER BY Id DESC;";

            using var connection = new NpgsqlConnection(_connectionString);
            return await connection.QueryAsync<RelatorioEntity>(query);
        }

        public async Task DeletarPorIdAsync(int id)
        {
            const string query = @"
                DELETE FROM Relatorio
                WHERE Id = @Id;";

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.ExecuteAsync(query, new { Id = id });
        }
    }
}
