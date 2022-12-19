using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SubterfugeCore.Models.GameEvents;

namespace SubterfugeServerConsole.Connections.Collections;

public class UserModelCollection : IDatabaseCollection<UserModel>
{
    private IMongoCollection<UserModel> _collection;

    public UserModelCollection(IMongoCollection<UserModel> collection)
    {
        this._collection = collection;
    }

    public async Task Upsert(UserModel item)
    {
        await _collection.ReplaceOneAsync(
            it => it.Id == item.Id,
            item,
            new UpdateOptions { IsUpsert = true }
        );
    }

    public async Task Delete(UserModel item)
    {
        await _collection.DeleteOneAsync(it => it.Id == item.Id);
    }

    public IMongoQueryable<UserModel> Query()
    {
        return _collection.AsQueryable();
    }

    public IEnumerable<CreateIndexModel<UserModel>> GetIndexes()
    {
        return new List<CreateIndexModel<UserModel>>()
        {
            new (Builders<UserModel>.IndexKeys.Ascending(user => user.Id)),
            new (Builders<UserModel>.IndexKeys.Ascending(user => user.DeviceIdentifier)),
            new (Builders<UserModel>.IndexKeys.Ascending(user => user.Username)),
            new (Builders<UserModel>.IndexKeys.Ascending(user => user.Email)),
        };
    }

    public void Flush()
    {
        _collection.DeleteMany(FilterDefinition<UserModel>.Empty);
    }
}