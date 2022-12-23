using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SubterfugeCore.Models.GameEvents;
using SubterfugeDatabaseProvider.Models;

namespace SubterfugeServerConsole.Connections.Collections;

public class SpecialistPackageCollection : IDatabaseCollection<DbSpecialistPackage>
{
    private IMongoCollection<DbSpecialistPackage> _collection;

    public SpecialistPackageCollection(IMongoCollection<DbSpecialistPackage> collection)
    {
        this._collection = collection;
    }

    public async Task Upsert(DbSpecialistPackage item)
    {
        await _collection.ReplaceOneAsync(
            it => it.Id == item.Id,
            item,
            new UpdateOptions { IsUpsert = true }
        );
    }

    public async Task Delete(DbSpecialistPackage item)
    {
        await _collection.DeleteOneAsync(it => it.Id == item.Id);
    }

    public IMongoQueryable<DbSpecialistPackage> Query()
    {
        return _collection.AsQueryable();
    }

    public async Task CreateIndexes()
    {
        await _collection.Indexes.CreateManyAsync( new List<CreateIndexModel<DbSpecialistPackage>>()
        {
            new CreateIndexModel<DbSpecialistPackage>(Builders<DbSpecialistPackage>.IndexKeys.Hashed(package => package.Creator.Id)),
            new CreateIndexModel<DbSpecialistPackage>(Builders<DbSpecialistPackage>.IndexKeys.Text(package => package.PackageName)),
        });
    }

    public void Flush()
    {
        _collection.DeleteMany(FilterDefinition<DbSpecialistPackage>.Empty);
    }
}