using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SubterfugeDatabaseProvider.Models;

namespace SubterfugeServerConsole.Connections.Collections;

public class ChatMessageCollection : IDatabaseCollection<DbChatMessage>
{
    private IMongoCollection<DbChatMessage> _collection;

    public ChatMessageCollection(IMongoCollection<DbChatMessage> collection)
    {
        this._collection = collection;
    }

    public async Task Upsert(DbChatMessage item)
    {
        await _collection.ReplaceOneAsync(
            it => it.Id == item.Id,
            item,
            new UpdateOptions { IsUpsert = true }
        );
    }

    public async Task Delete(DbChatMessage item)
    {
        await _collection.DeleteOneAsync(it => it.Id == item.Id);
    }

    public IMongoQueryable<DbChatMessage> Query()
    {
        return _collection.AsQueryable();
    }

    public async Task CreateIndexes()
    {
        await _collection.Indexes.CreateManyAsync(new List<CreateIndexModel<DbChatMessage>>()
        {
            new CreateIndexModel<DbChatMessage>(Builders<DbChatMessage>.IndexKeys.Ascending(message => message.RoomId)),
            new CreateIndexModel<DbChatMessage>(Builders<DbChatMessage>.IndexKeys.Ascending(message => message.GroupId)),
            new CreateIndexModel<DbChatMessage>(Builders<DbChatMessage>.IndexKeys.Ascending(message => message.SentBy)),
            new CreateIndexModel<DbChatMessage>(Builders<DbChatMessage>.IndexKeys.Ascending(message => message.CreatedAt)),
            new CreateIndexModel<DbChatMessage>(Builders<DbChatMessage>.IndexKeys.Text(message => message.Message)),
            new CreateIndexModel<DbChatMessage>(Builders<DbChatMessage>.IndexKeys.Text(message => message.ExpiresAt), new CreateIndexOptions()
            {
                ExpireAfter = TimeSpan.FromSeconds(0),
                Name = "ExpireAtIndex"
            }),
        });
    }

    public void Flush()
    {
        _collection.DeleteMany(FilterDefinition<DbChatMessage>.Empty);
    }
}