using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SubterfugeCore.Models.GameEvents;

namespace SubterfugeServerConsole.Connections.Collections;

public class ServerExceptionLogCollection: IDatabaseCollection<ServerExceptionLog>
{
    private IMongoCollection<ServerExceptionLog> _collection;

    public ServerExceptionLogCollection(IMongoCollection<ServerExceptionLog> collection)
    {
        this._collection = collection;
    }

    public async Task Upsert(ServerExceptionLog item)
    {
        await _collection.ReplaceOneAsync(
            it => it.Id == item.Id,
            item,
            new UpdateOptions { IsUpsert = true }
        );
    }

    public async Task Delete(ServerExceptionLog item)
    {
        await _collection.DeleteOneAsync(it => it.Id == item.Id);
    }

    public IMongoQueryable<ServerExceptionLog> Query()
    {
        return _collection.AsQueryable();
    }

    public IEnumerable<CreateIndexModel<ServerExceptionLog>> GetIndexes()
    {
        return new List<CreateIndexModel<ServerExceptionLog>>()
        {
                new CreateIndexModel<ServerExceptionLog>(Builders<ServerExceptionLog>.IndexKeys.Ascending(action => action.Id)),
                new CreateIndexModel<ServerExceptionLog>(Builders<ServerExceptionLog>.IndexKeys.Ascending(action => action.Username)),
                new CreateIndexModel<ServerExceptionLog>(Builders<ServerExceptionLog>.IndexKeys.Ascending(action => action.UserId)),
                new CreateIndexModel<ServerExceptionLog>(Builders<ServerExceptionLog>.IndexKeys.Ascending(action => action.RequestUri)),
                new CreateIndexModel<ServerExceptionLog>(Builders<ServerExceptionLog>.IndexKeys.Ascending(action => action.RemoteIpAddress)),
        };
    }

    public void Flush()
    {
        _collection.DeleteMany(FilterDefinition<ServerExceptionLog>.Empty);
    }
}