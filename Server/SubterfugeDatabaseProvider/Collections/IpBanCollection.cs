using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SubterfugeDatabaseProvider.Models;

namespace SubterfugeServerConsole.Connections.Collections;

public class IpBanCollection : IDatabaseCollection<DbIpBan>
{
    private IMongoCollection<DbIpBan> _collection;

    public IpBanCollection(IMongoCollection<DbIpBan> collection)
    {
        this._collection = collection;
    }
    
    public void Flush()
    {
        _collection.DeleteMany(FilterDefinition<DbIpBan>.Empty);
    }

    public async Task CreateIndexes()
    {
        await _collection.Indexes.CreateManyAsync( new List<CreateIndexModel<DbIpBan>>()
        {
            new CreateIndexModel<DbIpBan>(Builders<DbIpBan>.IndexKeys.Ascending(ban => ban.BannedUntil)),
            new CreateIndexModel<DbIpBan>(keys: Builders<DbIpBan>.IndexKeys.Text(ban => ban.IpAddressOrRegex)),
        });
    }

    public async Task Upsert(DbIpBan item)
    {
        await _collection.ReplaceOneAsync(
            it => it.Id == item.Id,
            item,
            new ReplaceOptions() { IsUpsert = true}
        );
    }

    public async Task Delete(DbIpBan item)
    {
        await _collection.DeleteOneAsync(it => it.Id == item.Id);
    }

    public IMongoQueryable<DbIpBan> Query()
    {
        return _collection.AsQueryable();
    }
}