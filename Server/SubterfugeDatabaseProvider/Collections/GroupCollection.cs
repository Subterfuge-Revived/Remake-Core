using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SubterfugeDatabaseProvider.Models;

namespace SubterfugeServerConsole.Connections.Collections;

public class GroupCollection : IDatabaseCollection<DbMessageGroup>
{
    private IMongoCollection<DbMessageGroup> _collection;

    public GroupCollection(IMongoCollection<DbMessageGroup> collection)
    {
        this._collection = collection;
    }

    public async Task Upsert(DbMessageGroup item)
    {
        await _collection.ReplaceOneAsync(
            it => it.Id == item.Id,
            item,
            new UpdateOptions { IsUpsert = true }
        );
    }

    public async Task Delete(DbMessageGroup item)
    {
        await _collection.DeleteOneAsync(it => it.Id == item.Id);
    }

    public IMongoQueryable<DbMessageGroup> Query()
    {
        return _collection.AsQueryable();
    }

    public async Task CreateIndexes()
    {
        await _collection.Indexes.CreateManyAsync( new List<CreateIndexModel<DbMessageGroup>>()
        {
            new CreateIndexModel<DbMessageGroup>(Builders<DbMessageGroup>.IndexKeys.Hashed(group => group.RoomId)),
            new CreateIndexModel<DbMessageGroup>(keys: Builders<DbMessageGroup>.IndexKeys.Ascending(room => room.ExpiresAt), options: new CreateIndexOptions()
            {
                ExpireAfter = TimeSpan.FromSeconds(0),
            }),
        });
    }

    public void Flush()
    {
        _collection.DeleteMany(FilterDefinition<DbMessageGroup>.Empty);
    }
}