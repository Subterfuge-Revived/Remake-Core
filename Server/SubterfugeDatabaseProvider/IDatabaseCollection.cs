using DnsClient.Internal;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SubterfugeDatabaseProvider.Models;
using SubterfugeServerConsole.Connections.Collections;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace SubterfugeServerConsole.Connections;

public interface IGenericDatabaseCollection
{
    void Flush();
    Task CreateIndexes();
}

public interface IDatabaseCollection<T> : IGenericDatabaseCollection
{
    Task Upsert(T item);
    Task Delete(T item);
    IMongoQueryable<T> Query();
}

public abstract class IDatabaseCollectionProvider {
    
    private readonly Dictionary<Type, IGenericDatabaseCollection> _componentMap = new Dictionary<Type, IGenericDatabaseCollection>();

    public void AddComponent<T>(IDatabaseCollection<T> component)
    {
        _componentMap[typeof(T)] = component;
    }

    public IDatabaseCollection<T> GetCollection<T>()
    {
        if (_componentMap.ContainsKey(typeof(T)))
        {
            return _componentMap[typeof(T)] as IDatabaseCollection<T>;
        }
        return null;
    }

    protected async Task CreateAllIndexes(ILogger logger)
    {
        var tasks = new List<Task>();
        foreach (var collection in _componentMap.Values)
        {
            tasks.Add(collection.CreateIndexes());
        }

        var task = Task.WhenAll(tasks);
        
        try
        {
            await task;
        }
        catch (Exception)
        {
            if (task.Exception != null)
            {
                logger.LogError($"Failed to create index. {task.Exception.Message} {task.Exception.StackTrace}");
                throw task.Exception;
            }
        }
    }

    public void FlushAll()
    {
        foreach (var collection in _componentMap.Values)
        {
            collection.Flush();
        }
    }
}