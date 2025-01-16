using LimpezaDeBase.Modelos;
using LimpezaDeBase.Modelos.Mongo;
using MongoDB.Bson;

namespace LimpezaDeBase.Services.Interfaces
{
    public interface IMongoService
    {
        Task AdicionarClienteAsync(ClienteDB cliente);
        Task AdicionarDocumentoAsync(BaseVerificada documento);
        Task AdicionarNotificacaoDadosAsync(NotificacaoDadosDB notificacaoDados);
        Task AdicionarProcessamentoAsync(ProcessamentoMongo processamento);
        Task AtualizarCatalogoDBAsync(ObjectId id, CatalgoContatoDB catalogoAtualizado);
        Task AtualizarOptOutDBAsync(ObjectId id, OptOutDB optoutAtualizado);
        Task InserirCatalogoDBAsync(CatalgoContatoDB catalogo);
        Task InserirCatalogosDBAsync(List<CatalgoContatoDB> catalogos);
        Task InserirOptOutAsync(OptOutDB optOutDB);
        Task<List<CatalgoContatoDB>> ObterCatalogoDBAsync();
        Task<List<BaseVerificada>> ObterDocumentoPorProcessIdAsync(string processId);
        Task<List<OptOutDB>> ObterOptOutPorContrato(string contrato);
        Task<List<ProcessamentoMongo>> ObterProcessamentosPendentesAsync();
        Task<List<ClienteDB>> ObterTodosClientesAsync();
        Task<List<NotificacaoDadosDB>> ObterTodosDadosDeNotificacaoAsync();
        Task<List<BaseVerificada>> ObterTodosDocumentosAsync();
        Task RemoverClienteAsync(ObjectId id);
        Task RemoverDocumentoAsync(ObjectId id);
        Task RemoverProcessamentoAsync(ObjectId id);
    }
}
