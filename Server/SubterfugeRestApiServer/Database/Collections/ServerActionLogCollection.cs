using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SubterfugeCore.Models.GameEvents;

namespace SubterfugeServerConsole.Connections.Collections;

public class ServerActionLogCollection : IDatabaseCollection<ServerActionLog>
{
    private IMongoCollection<ServerActionLog> _collection;

    public ServerActionLogCollection(IMongoCollection<ServerActionLog> collection)
    {
        this._collection = collection;
    }

    public async Task Upsert(ServerActionLog item)
    {
        await _collection.ReplaceOneAsync(
            it => it.Id == item.Id,
            item,
            new UpdateOptions { IsUpsert = true }
        );
    }

    public async Task Delete(ServerActionLog item)
    {
        await _collection.DeleteOneAsync(it => it.Id == item.Id);
    }

    public IMongoQueryable<ServerActionLog> Query()
    {
        return _collection.AsQueryable();
    }

    public IEnumerable<CreateIndexModel<ServerActionLog>> GetIndexes()
    {
        return new List<CreateIndexModel<ServerActionLog>>()
        {
            new CreateIndexModel<ServerActionLog>(Builders<ServerActionLog>.IndexKeys.Ascending(action => action.Id)),
            new CreateIndexModel<ServerActionLog>(Builders<ServerActionLog>.IndexKeys.Ascending(action => action.Username)),
            new CreateIndexModel<ServerActionLog>(Builders<ServerActionLog>.IndexKeys.Ascending(action => action.UserId)),
            new CreateIndexModel<ServerActionLog>(Builders<ServerActionLog>.IndexKeys.Ascending(action => action.RequestUrl)),
            new CreateIndexModel<ServerActionLog>(Builders<ServerActionLog>.IndexKeys.Ascending(action => action.StatusCode)),
            new CreateIndexModel<ServerActionLog>(Builders<ServerActionLog>.IndexKeys.Ascending(action => action.UnixTimeProcessed)),
        };
    }

    public void Flush()
    {
        _collection.DeleteMany(FilterDefinition<ServerActionLog>.Empty);
    }
}