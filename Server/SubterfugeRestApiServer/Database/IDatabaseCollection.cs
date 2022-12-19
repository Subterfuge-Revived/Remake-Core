using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace SubterfugeServerConsole.Connections;

public interface IDatabaseCollection<T>
{
    Task Upsert(T item);
    Task Delete(T item);
    IMongoQueryable<T> Query();
    IEnumerable<CreateIndexModel<T>> GetIndexes();
    void Flush();
}