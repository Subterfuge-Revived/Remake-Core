using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SubterfugeCore.Models.GameEvents;

namespace SubterfugeServerConsole.Connections.Collections;

public class GameEventCollection : IDatabaseCollection<GameEventData>
{
    private IMongoCollection<GameEventData> _collection;

    public GameEventCollection(IMongoCollection<GameEventData> collection)
    {
        this._collection = collection;
    }

    public async Task Upsert(GameEventData item)
    {
        await _collection.ReplaceOneAsync(
            it => it.Id == item.Id,
            item,
            new UpdateOptions { IsUpsert = true }
        );
    }

    public async Task Delete(GameEventData item)
    {
        await _collection.DeleteOneAsync(it => it.Id == item.Id);
    }

    public IMongoQueryable<GameEventData> Query()
    {
        return _collection.AsQueryable();
    }

    public IEnumerable<CreateIndexModel<GameEventData>> GetIndexes()
    {
        return new List<CreateIndexModel<GameEventData>>()
        {
            new CreateIndexModel<GameEventData>(Builders<GameEventData>.IndexKeys.Ascending(gameEvent => gameEvent.Id)),
            new CreateIndexModel<GameEventData>(Builders<GameEventData>.IndexKeys.Ascending(gameEvent => gameEvent.IssuedBy)),
            new CreateIndexModel<GameEventData>(Builders<GameEventData>.IndexKeys.Ascending(gameEvent => gameEvent.UnixTimeIssued)),
            new CreateIndexModel<GameEventData>(Builders<GameEventData>.IndexKeys.Ascending(gameEvent => gameEvent.OccursAtTick)),
        };
    }

    public void Flush()
    {
        _collection.DeleteMany(FilterDefinition<GameEventData>.Empty);
    }
}