using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SubterfugeCore.Models.GameEvents;

namespace SubterfugeServerConsole.Connections.Collections;

public class GroupCollection : IDatabaseCollection<MessageGroupDatabaseModel>
{
    private IMongoCollection<MessageGroupDatabaseModel> _collection;

    public GroupCollection(IMongoCollection<MessageGroupDatabaseModel> collection)
    {
        this._collection = collection;
    }

    public async Task Upsert(MessageGroupDatabaseModel item)
    {
        await _collection.ReplaceOneAsync(
            it => it.Id == item.Id,
            item,
            new UpdateOptions { IsUpsert = true }
        );
    }

    public async Task Delete(MessageGroupDatabaseModel item)
    {
        await _collection.DeleteOneAsync(it => it.Id == item.Id);
    }

    public IMongoQueryable<MessageGroupDatabaseModel> Query()
    {
        return _collection.AsQueryable();
    }

    public IEnumerable<CreateIndexModel<MessageGroupDatabaseModel>> GetIndexes()
    {
        return new List<CreateIndexModel<MessageGroupDatabaseModel>>()
        {
            new CreateIndexModel<MessageGroupDatabaseModel>(Builders<MessageGroupDatabaseModel>.IndexKeys.Ascending(group => group.Id)),
            new CreateIndexModel<MessageGroupDatabaseModel>(Builders<MessageGroupDatabaseModel>.IndexKeys.Ascending(group => group.RoomId)),
        };
    }

    public void Flush()
    {
        _collection.DeleteMany(FilterDefinition<MessageGroupDatabaseModel>.Empty);
    }
}