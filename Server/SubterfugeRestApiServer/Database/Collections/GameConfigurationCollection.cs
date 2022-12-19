using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SubterfugeCore.Models.GameEvents;

namespace SubterfugeServerConsole.Connections.Collections;

public class GameConfigurationCollection : IDatabaseCollection<GameConfiguration>
{
    private IMongoCollection<GameConfiguration> _collection;

    public GameConfigurationCollection(IMongoCollection<GameConfiguration> collection)
    {
        this._collection = collection;
    }

    public async Task Upsert(GameConfiguration item)
    {
        await _collection.ReplaceOneAsync(
            it => it.Id == item.Id,
            item,
            new UpdateOptions { IsUpsert = true }
        );
    }

    public async Task Delete(GameConfiguration item)
    {
        await _collection.DeleteOneAsync(it => it.Id == item.Id);
    }

    public IMongoQueryable<GameConfiguration> Query()
    {
        return _collection.AsQueryable();
    }

    public IEnumerable<CreateIndexModel<GameConfiguration>> GetIndexes()
    {
        return new List<CreateIndexModel<GameConfiguration>>()
        {
            new CreateIndexModel<GameConfiguration>(Builders<GameConfiguration>.IndexKeys.Ascending(room => room.Id)),
            new CreateIndexModel<GameConfiguration>(Builders<GameConfiguration>.IndexKeys.Ascending(room => room.RoomName)),
            new CreateIndexModel<GameConfiguration>(Builders<GameConfiguration>.IndexKeys.Ascending(room => room.UnixTimeCreated)),
            new CreateIndexModel<GameConfiguration>(Builders<GameConfiguration>.IndexKeys.Ascending(room => room.Creator.Id)),
            new CreateIndexModel<GameConfiguration>(Builders<GameConfiguration>.IndexKeys.Ascending(room => room.Creator.Username)),
            new CreateIndexModel<GameConfiguration>(Builders<GameConfiguration>.IndexKeys.Ascending(room => room.GameVersion)),
            new CreateIndexModel<GameConfiguration>(Builders<GameConfiguration>.IndexKeys.Ascending(room => room.RoomStatus)),
        };
    }

    public void Flush()
    {
        _collection.DeleteMany(FilterDefinition<GameConfiguration>.Empty);
    }
}