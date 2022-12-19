using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SubterfugeCore.Models.GameEvents;

namespace SubterfugeServerConsole.Connections.Collections;

public class SpecialistConfigurationCollection : IDatabaseCollection<SpecialistConfiguration>
{
    private IMongoCollection<SpecialistConfiguration> _collection;

    public SpecialistConfigurationCollection(IMongoCollection<SpecialistConfiguration> collection)
    {
        this._collection = collection;
    }

    public async Task Upsert(SpecialistConfiguration item)
    {
        await _collection.ReplaceOneAsync(
            it => it.Id == item.Id,
            item,
            new UpdateOptions { IsUpsert = true }
        );
    }

    public async Task Delete(SpecialistConfiguration item)
    {
        await _collection.DeleteOneAsync(it => it.Id == item.Id);
    }

    public IMongoQueryable<SpecialistConfiguration> Query()
    {
        return _collection.AsQueryable();
    }

    public IEnumerable<CreateIndexModel<SpecialistConfiguration>> GetIndexes()
    {
        return new List<CreateIndexModel<SpecialistConfiguration>>()
        {
            new CreateIndexModel<SpecialistConfiguration>(Builders<SpecialistConfiguration>.IndexKeys.Ascending(spec => spec.Id)),
            new CreateIndexModel<SpecialistConfiguration>(Builders<SpecialistConfiguration>.IndexKeys.Ascending(spec => spec.Creator.Id)),
            new CreateIndexModel<SpecialistConfiguration>(Builders<SpecialistConfiguration>.IndexKeys.Ascending(spec => spec.PromotesFromSpecialistId)),
            new CreateIndexModel<SpecialistConfiguration>(Builders<SpecialistConfiguration>.IndexKeys.Text(spec => spec.SpecialistName)),
        };
    }

    public void Flush()
    {
        _collection.DeleteMany(FilterDefinition<SpecialistConfiguration>.Empty);
    }
}