using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SubterfugeCore.Models.GameEvents;
using SubterfugeDatabaseProvider.Models;

namespace SubterfugeServerConsole.Connections.Collections;

public class ServerActionLogCollection : IDatabaseCollection<DbServerAction>
{
    private IMongoCollection<DbServerAction> _collection;

    public ServerActionLogCollection(IMongoCollection<DbServerAction> collection)
    {
        this._collection = collection;
    }

    public async Task Upsert(DbServerAction item)
    {
        await _collection.ReplaceOneAsync(
            it => it.Id == item.Id,
            item,
            new UpdateOptions { IsUpsert = true }
        );
    }

    public async Task Delete(DbServerAction item)
    {
        await _collection.DeleteOneAsync(it => it.Id == item.Id);
    }

    public IMongoQueryable<DbServerAction> Query()
    {
        return _collection.AsQueryable();
    }

    public async Task CreateIndexes()
    {
        await _collection.Indexes.CreateManyAsync( new List<CreateIndexModel<DbServerAction>>()
        {
            new CreateIndexModel<DbServerAction>(Builders<DbServerAction>.IndexKeys.Ascending(action => action.Id)),
            new CreateIndexModel<DbServerAction>(Builders<DbServerAction>.IndexKeys.Ascending(action => action.Username)),
            new CreateIndexModel<DbServerAction>(Builders<DbServerAction>.IndexKeys.Ascending(action => action.UserId)),
            new CreateIndexModel<DbServerAction>(Builders<DbServerAction>.IndexKeys.Ascending(action => action.RemoteIpAddress)),
            new CreateIndexModel<DbServerAction>(Builders<DbServerAction>.IndexKeys.Ascending(action => action.RequestUrl)),
            new CreateIndexModel<DbServerAction>(Builders<DbServerAction>.IndexKeys.Ascending(action => action.StatusCode)),
            new CreateIndexModel<DbServerAction>(keys: Builders<DbServerAction>.IndexKeys.Ascending(room => room.ExpiresAt), options: new CreateIndexOptions()
            {
                ExpireAfter = TimeSpan.FromSeconds(0),
                Name = "ExpireAtIndex"
            }),
        });
    }

    public void Flush()
    {
        _collection.DeleteMany(FilterDefinition<DbServerAction>.Empty);
    }
}