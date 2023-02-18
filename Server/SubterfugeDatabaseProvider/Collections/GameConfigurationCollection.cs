using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Subterfuge.Remake.Server.Database.Models;

namespace Subterfuge.Remake.Server.Database.Collections;

public class GameConfigurationCollection : IDatabaseCollection<DbGameLobbyConfiguration>
{
    private IMongoCollection<DbGameLobbyConfiguration> _collection;

    public GameConfigurationCollection(IMongoCollection<DbGameLobbyConfiguration> collection)
    {
        this._collection = collection;
    }

    public async Task Upsert(DbGameLobbyConfiguration item)
    {
        await _collection.ReplaceOneAsync(
            it => it.Id == item.Id,
            item,
            new ReplaceOptions() { IsUpsert = true}
        );
    }

    public async Task Delete(DbGameLobbyConfiguration item)
    {
        await _collection.DeleteOneAsync(it => it.Id == item.Id);
    }

    public IMongoQueryable<DbGameLobbyConfiguration> Query()
    {
        return _collection.AsQueryable();
    }

    public async Task CreateIndexes()
    {
        await _collection.Indexes.CreateManyAsync( new List<CreateIndexModel<DbGameLobbyConfiguration>>()
        {
            new CreateIndexModel<DbGameLobbyConfiguration>(Builders<DbGameLobbyConfiguration>.IndexKeys.Text(room => room.RoomName)),
            new CreateIndexModel<DbGameLobbyConfiguration>(Builders<DbGameLobbyConfiguration>.IndexKeys.Ascending(room => room.TimeCreated)),
            new CreateIndexModel<DbGameLobbyConfiguration>(Builders<DbGameLobbyConfiguration>.IndexKeys.Hashed(room => room.Creator.Id)),
            new CreateIndexModel<DbGameLobbyConfiguration>(Builders<DbGameLobbyConfiguration>.IndexKeys.Ascending(room => room.GameVersion)),
            new CreateIndexModel<DbGameLobbyConfiguration>(Builders<DbGameLobbyConfiguration>.IndexKeys.Ascending(room => room.RoomStatus)),
            new CreateIndexModel<DbGameLobbyConfiguration>(Builders<DbGameLobbyConfiguration>.IndexKeys.Ascending(room => room.ExpiresAt), options: new CreateIndexOptions()
            {
                ExpireAfter = TimeSpan.FromSeconds(0),
            }),
        });
    }

    public void Flush()
    {
        _collection.DeleteMany(FilterDefinition<DbGameLobbyConfiguration>.Empty);
    }
}