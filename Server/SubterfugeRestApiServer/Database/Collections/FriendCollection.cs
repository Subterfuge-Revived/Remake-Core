using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SubterfugeCore.Models.GameEvents;

namespace SubterfugeServerConsole.Connections.Collections;

public class FriendCollection : IDatabaseCollection<Friend>
{
    private IMongoCollection<Friend> _collection;

    public FriendCollection(IMongoCollection<Friend> collection)
    {
        this._collection = collection;
    }

    public async Task Upsert(Friend item)
    {
        await _collection.ReplaceOneAsync(
            it => it.Id == item.Id,
            item,
            new UpdateOptions { IsUpsert = true }
        );
    }

    public async Task Delete(Friend item)
    {
        await _collection.DeleteOneAsync(it => it.Id == item.Id);
    }

    public IMongoQueryable<Friend> Query()
    {
        return _collection.AsQueryable();
    }

    public IEnumerable<CreateIndexModel<Friend>> GetIndexes()
    {
        return new List<CreateIndexModel<Friend>>()
        {
            new CreateIndexModel<Friend>(Builders<Friend>.IndexKeys.Ascending(relation => relation.PlayerId)),
            new CreateIndexModel<Friend>(Builders<Friend>.IndexKeys.Ascending(relation => relation.FriendId)),
            new CreateIndexModel<Friend>(Builders<Friend>.IndexKeys.Ascending(relation => relation.RelationshipStatus)),
        };
    }

    public void Flush()
    {
        _collection.DeleteMany(FilterDefinition<Friend>.Empty);
    }
}