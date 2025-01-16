using LimpezaDeBase.Modelos;
using LimpezaDeBase.Modelos.Mongo;
using LimpezaDeBase.Services.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Diagnostics;

namespace LimpezaDeBase.Services
{
    public class MongoService : IMongoService
    {
        private readonly IMongoCollection<BaseVerificada> _contatosCollection;
        private readonly IMongoCollection<ProcessamentoMongo> _processamentosCollection;
        private readonly IMongoCollection<CatalgoContatoDB> _catalogoContatoDB;
        private readonly IMongoCollection<ClienteDB> _clienteDB;
        private readonly IMongoCollection<OptOutDB> _optOutDB;
        private readonly IMongoCollection<NotificacaoDadosDB> _notificacaoDB;

        public MongoService(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("Produtos");

            _contatosCollection = database.GetCollection<BaseVerificada>("LimpezaBase_dados");
            _processamentosCollection = database.GetCollection<ProcessamentoMongo>("LimpezaBase_filaprocessamento");
            _catalogoContatoDB = database.GetCollection<CatalgoContatoDB>("LimpezaBase_catalogocontato");
            _clienteDB = database.GetCollection<ClienteDB>("LimpezaBase_clientes");
            _optOutDB = database.GetCollection<OptOutDB>("LimpezaBase_optout");
            _notificacaoDB = database.GetCollection<NotificacaoDadosDB>("LimpezaBase_notificacao");
        }
        public async Task AdicionarNotificacaoDadosAsync(NotificacaoDadosDB notificacaoDados) => await _notificacaoDB.InsertOneAsync(notificacaoDados);
        public async Task<List<NotificacaoDadosDB>> ObterTodosDadosDeNotificacaoAsync() => await _notificacaoDB.Find(new BsonDocument()).ToListAsync();

        public async Task AdicionarClienteAsync(ClienteDB cliente) => await _clienteDB.InsertOneAsync(cliente);
        public async Task<List<ClienteDB>> ObterTodosClientesAsync() => await _clienteDB.Find(new BsonDocument()).ToListAsync();

        public async Task RemoverClienteAsync(ObjectId id)
        {
            var filtro = Builders<ClienteDB>.Filter.Eq(doc => doc.Id, id);
            await _clienteDB.DeleteOneAsync(filtro);
        }

        public async Task AdicionarDocumentoAsync(BaseVerificada documento) => await _contatosCollection.InsertOneAsync(documento);

        public async Task RemoverDocumentoAsync(ObjectId id)
        {
            var filtro = Builders<BaseVerificada>.Filter.Eq(doc => doc.Id, id);
            await _contatosCollection.DeleteOneAsync(filtro);
        }

        public async Task<List<BaseVerificada>> ObterTodosDocumentosAsync() => await _contatosCollection.Find(new BsonDocument()).ToListAsync();

        public async Task<List<BaseVerificada>> ObterDocumentoPorProcessIdAsync(string processId)
        {
            var filtro = Builders<BaseVerificada>.Filter.Eq(doc => doc.ProcessId, processId);
            return await _contatosCollection.Find(filtro).ToListAsync();
        }

        public async Task AdicionarProcessamentoAsync(ProcessamentoMongo processamento) => await _processamentosCollection.InsertOneAsync(processamento);

        public async Task<List<ProcessamentoMongo>> ObterProcessamentosPendentesAsync() => await _processamentosCollection.Find(new BsonDocument()).ToListAsync();

        public async Task RemoverProcessamentoAsync(ObjectId id)
        {
            var filtro = Builders<ProcessamentoMongo>.Filter.Eq(doc => doc.Id, id);
            await _processamentosCollection.DeleteOneAsync(filtro);
        }

        public async Task<List<CatalgoContatoDB>> ObterCatalogoDBAsync() => await _catalogoContatoDB.Find(new BsonDocument()).ToListAsync();

        public async Task InserirCatalogoDBAsync(CatalgoContatoDB catalogo) => await _catalogoContatoDB.InsertOneAsync(catalogo);

        public async Task InserirCatalogosDBAsync(List<CatalgoContatoDB> catalogos) => await _catalogoContatoDB.InsertManyAsync(catalogos);

        public async Task AtualizarCatalogoDBAsync(ObjectId id, CatalgoContatoDB catalogoAtualizado)
        {
            var filtro = Builders<CatalgoContatoDB>.Filter.Eq(c => c.Id, id);
            var resultado = await _catalogoContatoDB.ReplaceOneAsync(filtro, catalogoAtualizado);

            if (resultado.ModifiedCount == 0)
            {
                throw new Exception("Nenhum documento foi atualizado.");
            }
        }

        public async Task<List<OptOutDB>> ObterOptOutPorContrato(string contrato)
        {
            var filtro = Builders<OptOutDB>.Filter.Eq(doc => doc.Contrato, contrato);
            return await _optOutDB.Find(filtro).ToListAsync();
        }

        public async Task InserirOptOutAsync(OptOutDB optOutDB) => await _optOutDB.InsertOneAsync(optOutDB);

        public async Task AtualizarOptOutDBAsync(ObjectId id, OptOutDB optoutAtualizado)
        {
            var filtro = Builders<OptOutDB>.Filter.Eq(c => c.Id, id);
            var resultado = await _optOutDB.ReplaceOneAsync(filtro, optoutAtualizado);

            if (resultado.ModifiedCount == 0)
            {
                throw new Exception("Nenhum documento foi atualizado.");
            }
        }
    }
}
