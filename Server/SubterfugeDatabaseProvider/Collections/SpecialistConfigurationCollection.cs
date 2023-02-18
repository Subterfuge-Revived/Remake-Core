using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Subterfuge.Remake.Server.Database.Models;

namespace Subterfuge.Remake.Server.Database.Collections;

public class SpecialistConfigurationCollection : IDatabaseCollection<DbSpecialistConfiguration>
{
    private IMongoCollection<DbSpecialistConfiguration> _collection;

    public SpecialistConfigurationCollection(IMongoCollection<DbSpecialistConfiguration> collection)
    {
        this._collection = collection;
    }

    public async Task Upsert(DbSpecialistConfiguration item)
    {
        await _collection.ReplaceOneAsync(
            it => it.Id == item.Id,
            item,
            new ReplaceOptions() { IsUpsert = true}
        );
    }

    public async Task Delete(DbSpecialistConfiguration item)
    {
        await _collection.DeleteOneAsync(it => it.Id == item.Id);
    }

    public IMongoQueryable<DbSpecialistConfiguration> Query()
    {
        return _collection.AsQueryable();
    }

    public async Task CreateIndexes()
    {
        await _collection.Indexes.CreateManyAsync( new List<CreateIndexModel<DbSpecialistConfiguration>>()
        {
            new CreateIndexModel<DbSpecialistConfiguration>(Builders<DbSpecialistConfiguration>.IndexKeys.Hashed(spec => spec.Creator.Id)),
            new CreateIndexModel<DbSpecialistConfiguration>(Builders<DbSpecialistConfiguration>.IndexKeys.Hashed(spec => spec.PromotesFromSpecialistId)),
            new CreateIndexModel<DbSpecialistConfiguration>(Builders<DbSpecialistConfiguration>.IndexKeys.Text(spec => spec.SpecialistName)),
        });
    }

    public void Flush()
    {
        _collection.DeleteMany(FilterDefinition<DbSpecialistConfiguration>.Empty);
    }
}