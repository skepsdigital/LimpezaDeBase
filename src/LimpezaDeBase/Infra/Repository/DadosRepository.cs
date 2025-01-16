using LimpezaDeBase.Modelos;
using LimpezaDeBase.Modelos.Mongo;
using MongoDB.Driver;

namespace LimpezaDeBase.Infra.Repository
{
    public class DadosRepository : Repository<ContatoCallbackObjetoMongo>
    {
        public DadosRepository(IMongoClient mongoClient) : base(mongoClient, "Produtos", "LimpezaBase_dados")
        {
        }
    }
}
