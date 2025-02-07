using Dapper;
using LimpezaDeBase.Infra.Interfaces;
using LimpezaDeBase.Modelos.Entidades;
using Npgsql;

namespace LimpezaDeBase.Infra.Repository
{
    public class ProcessamentoRepository : IProcessamentoRepository
    {
        private readonly string _connectionString;

        public ProcessamentoRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> AdicionarAsync(ProcessamentoEntity processamento)
        {
            try
            {
                const string query = @"
                INSERT INTO Processamento 
                    (NumeroDeContatosTotal, ProcessId, Email, Contrato, Roteador, Data, FoiProcessado, EnviarNotificacao, Funcionalidade)
                VALUES 
                    (@NumeroDeContatosTotal, @ProcessId::uuid, @Email, @Contrato, @Roteador, @Data, @FoiProcessado, @EnviarNotificacao, @Funcionalidade)
                RETURNING ID;";

                using var connection = new NpgsqlConnection(_connectionString);
                return await connection.ExecuteScalarAsync<int>(query, processamento);
            }
            catch(Exception ex)
            {
                return 0;
            }
        }

        public async Task AtualizarAsync(ProcessamentoEntity processamento)
        {
            const string query = @"
                UPDATE Processamento
                SET
                    FoiProcessado = @FoiProcessado
                WHERE ID = @ID;";

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.ExecuteAsync(query, processamento);
        }

        public async Task<IEnumerable<ProcessamentoEntity>> BuscarTodosProcessamentosPendentesAsync()
        {
            const string query = @"
                SELECT
            ID,
            ProcessId::TEXT AS ProcessId, -- Converte UUID para texto
            NumeroDeContatosTotal,
            Email,
            Contrato,
            Roteador,
            Data,
            FoiProcessado,
            EnviarNotificacao
                FROM Processamento
                WHERE FoiProcessado = FALSE
                ORDER BY Data DESC;";

            using var connection = new NpgsqlConnection(_connectionString);
            return await connection.QueryAsync<ProcessamentoEntity>(query);
        }
    }
}
