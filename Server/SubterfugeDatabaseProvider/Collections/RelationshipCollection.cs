using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SubterfugeDatabaseProvider.Models;

namespace SubterfugeServerConsole.Connections.Collections;

public class RelationshipCollection : IDatabaseCollection<DbPlayerRelationship>
{
    private IMongoCollection<DbPlayerRelationship> _collection;

    public RelationshipCollection(IMongoCollection<DbPlayerRelationship> collection)
    {
        this._collection = collection;
    }

    public async Task Upsert(DbPlayerRelationship item)
    {
        item.UnixTimeLastUpdated = DateTime.UtcNow;
        await _collection.ReplaceOneAsync(
            it => it.Id == item.Id,
            item,
            new UpdateOptions { IsUpsert = true }
        );
    }

    public async Task Delete(DbPlayerRelationship item)
    {
        await _collection.DeleteOneAsync(it => it.Id == item.Id);
    }

    public IMongoQueryable<DbPlayerRelationship> Query()
    {
        return _collection.AsQueryable();
    }

    public async Task CreateIndexes()
    {
        await _collection.Indexes.CreateManyAsync( new List<CreateIndexModel<DbPlayerRelationship>>()
        {
            new CreateIndexModel<DbPlayerRelationship>(Builders<DbPlayerRelationship>.IndexKeys.Ascending(relation => relation.Player)),
            new CreateIndexModel<DbPlayerRelationship>(Builders<DbPlayerRelationship>.IndexKeys.Ascending(relation => relation.Friend)),
            new CreateIndexModel<DbPlayerRelationship>(Builders<DbPlayerRelationship>.IndexKeys.Ascending(relation => relation.RelationshipStatus)),
        });
    }

    public void Flush()
    {
        _collection.DeleteMany(FilterDefinition<DbPlayerRelationship>.Empty);
    }
}