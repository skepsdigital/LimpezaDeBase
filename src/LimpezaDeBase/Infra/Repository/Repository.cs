using MongoDB.Bson;
using MongoDB.Driver;

public class Repository<T> where T : class
{
    private readonly IMongoCollection<T> _collection;

    public Repository(IMongoClient mongoClient, string databaseName, string collectionName)
    {
        var database = mongoClient.GetDatabase(databaseName);
        _collection = database.GetCollection<T>(collectionName);
    }

    // Operação de busca por todos os documentos
    public async Task<List<T>> GetAllAsync() =>
        await _collection.Find(new BsonDocument()).ToListAsync();

    // Operação de busca por ID
    public async Task<T> GetByIdAsync(ObjectId id) =>
        await _collection.Find(Builders<T>.Filter.Eq("_id", id)).FirstOrDefaultAsync();

    // Inserir um único documento
    public async Task InsertAsync(T entity) =>
        await _collection.InsertOneAsync(entity);

    // Inserir múltiplos documentos
    public async Task InsertManyAsync(IEnumerable<T> entities) =>
        await _collection.InsertManyAsync(entities);

    // Atualizar um documento por ID
    public async Task UpdateAsync(ObjectId id, T updatedEntity) =>
        await _collection.ReplaceOneAsync(Builders<T>.Filter.Eq("_id", id), updatedEntity);

    // Remover um documento por ID
    public async Task DeleteAsync(ObjectId id) =>
        await _collection.DeleteOneAsync(Builders<T>.Filter.Eq("_id", id));
}
