using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Subterfuge.Remake.Server.Database.Models;

namespace Subterfuge.Remake.Server.Database.Collections;



public class UserModelCollection : IDatabaseCollection<DbUserModel>
{
    private IMongoCollection<DbUserModel> _collection;

    public UserModelCollection(IMongoCollection<DbUserModel> collection)
    {
        this._collection = collection;
    }

    public async Task Upsert(DbUserModel item)
    {
        await _collection.ReplaceOneAsync(
            it => it.Id == item.Id,
            item,
            new ReplaceOptions() { IsUpsert = true}
        );
    }

    public async Task Delete(DbUserModel item)
    {
        await _collection.DeleteOneAsync(it => it.Id == item.Id);
    }

    public IMongoQueryable<DbUserModel> Query()
    {
        return _collection.AsQueryable();
    }

    public async Task CreateIndexes()
    {
        await _collection.Indexes.CreateManyAsync( new List<CreateIndexModel<DbUserModel>>()
        {
            new (Builders<DbUserModel>.IndexKeys.Hashed(user => user.DeviceIdentifier)),
            new (Builders<DbUserModel>.IndexKeys.Ascending(user => user.Username)),
            new (Builders<DbUserModel>.IndexKeys.Hashed(user => user.PhoneNumber)),
        });
    }

    public void Flush()
    {
        _collection.DeleteMany(FilterDefinition<DbUserModel>.Empty);
    }
}