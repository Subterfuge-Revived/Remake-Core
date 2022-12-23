using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SubterfugeDatabaseProvider.Models;

namespace SubterfugeServerConsole.Connections.Collections;

public class GameEventCollection : IDatabaseCollection<DbGameEvent>
{
    private IMongoCollection<DbGameEvent> _collection;

    public GameEventCollection(IMongoCollection<DbGameEvent> collection)
    {
        this._collection = collection;
    }

    public async Task Upsert(DbGameEvent item)
    {
        await _collection.ReplaceOneAsync(
            it => it.Id == item.Id,
            item,
            new UpdateOptions { IsUpsert = true }
        );
    }

    public async Task Delete(DbGameEvent item)
    {
        await _collection.DeleteOneAsync(it => it.Id == item.Id);
    }

    public IMongoQueryable<DbGameEvent> Query()
    {
        return _collection.AsQueryable();
    }

    public async Task CreateIndexes()
    {
        await _collection.Indexes.CreateManyAsync( new List<CreateIndexModel<DbGameEvent>>()
        {
            new CreateIndexModel<DbGameEvent>(Builders<DbGameEvent>.IndexKeys.Hashed(gameEvent => gameEvent.IssuedBy.Id)),
            new CreateIndexModel<DbGameEvent>(Builders<DbGameEvent>.IndexKeys.Ascending(gameEvent => gameEvent.TimeIssued)),
            new CreateIndexModel<DbGameEvent>(Builders<DbGameEvent>.IndexKeys.Ascending(gameEvent => gameEvent.OccursAtTick)),
            new CreateIndexModel<DbGameEvent>(keys: Builders<DbGameEvent>.IndexKeys.Ascending(room => room.ExpiresAt), options: new CreateIndexOptions()
            {
                ExpireAfter = TimeSpan.FromSeconds(0),
            }),
        });
    }

    public void Flush()
    {
        _collection.DeleteMany(FilterDefinition<DbGameEvent>.Empty);
    }
}