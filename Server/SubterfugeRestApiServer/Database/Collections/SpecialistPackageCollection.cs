using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SubterfugeCore.Models.GameEvents;

namespace SubterfugeServerConsole.Connections.Collections;

public class SpecialistPackageCollection : IDatabaseCollection<SpecialistPackage>
{
    private IMongoCollection<SpecialistPackage> _collection;

    public SpecialistPackageCollection(IMongoCollection<SpecialistPackage> collection)
    {
        this._collection = collection;
    }

    public async Task Upsert(SpecialistPackage item)
    {
        await _collection.ReplaceOneAsync(
            it => it.Id == item.Id,
            item,
            new UpdateOptions { IsUpsert = true }
        );
    }

    public async Task Delete(SpecialistPackage item)
    {
        await _collection.DeleteOneAsync(it => it.Id == item.Id);
    }

    public IMongoQueryable<SpecialistPackage> Query()
    {
        return _collection.AsQueryable();
    }

    public IEnumerable<CreateIndexModel<SpecialistPackage>> GetIndexes()
    {
        return new List<CreateIndexModel<SpecialistPackage>>()
        {
            new CreateIndexModel<SpecialistPackage>(Builders<SpecialistPackage>.IndexKeys.Ascending(package => package.Id)),
            new CreateIndexModel<SpecialistPackage>(Builders<SpecialistPackage>.IndexKeys.Ascending(package => package.Creator.Id)),
            new CreateIndexModel<SpecialistPackage>(Builders<SpecialistPackage>.IndexKeys.Text(package => package.PackageName)),
        };
    }

    public void Flush()
    {
        _collection.DeleteMany(FilterDefinition<SpecialistPackage>.Empty);
    }
}