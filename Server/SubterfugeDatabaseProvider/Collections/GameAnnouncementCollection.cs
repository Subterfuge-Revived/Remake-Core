using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Subterfuge.Remake.Server.Database.Models;

namespace Subterfuge.Remake.Server.Database.Collections;

public class GameAnnouncementCollection : IDatabaseCollection<DbGameAnnouncement>
{
    
    private IMongoCollection<DbGameAnnouncement> _collection;
    
    public GameAnnouncementCollection(IMongoCollection<DbGameAnnouncement> collection)
    {
        this._collection = collection;
    }
    
    public void Flush()
    {
        _collection.DeleteMany(FilterDefinition<DbGameAnnouncement>.Empty);
    }

    public async Task CreateIndexes()
    {
        await _collection.Indexes.CreateManyAsync(new List<CreateIndexModel<DbGameAnnouncement>>()
        {
            new CreateIndexModel<DbGameAnnouncement>(Builders<DbGameAnnouncement>.IndexKeys.Hashed(announcement => announcement.Id)),
            new CreateIndexModel<DbGameAnnouncement>(Builders<DbGameAnnouncement>.IndexKeys.Text(announcement => announcement.BroadcastTo)),
            new CreateIndexModel<DbGameAnnouncement>(Builders<DbGameAnnouncement>.IndexKeys.Ascending(announcement => announcement.StartsAt)),
            new CreateIndexModel<DbGameAnnouncement>(Builders<DbGameAnnouncement>.IndexKeys.Ascending(announcement => announcement.ExpiresAt), new CreateIndexOptions()
            {
                ExpireAfter = TimeSpan.FromSeconds(0),
            }),
        });
    }

    public async Task Upsert(DbGameAnnouncement item)
    {
        await _collection.ReplaceOneAsync(
            it => it.Id == item.Id,
            item,
            new ReplaceOptions() { IsUpsert = true}
        );
    }

    public async Task Delete(DbGameAnnouncement item)
    {
        await _collection.DeleteOneAsync(it => it.Id == item.Id);
    }

    public IMongoQueryable<DbGameAnnouncement> Query()
    {
        return _collection.AsQueryable();
    }
}