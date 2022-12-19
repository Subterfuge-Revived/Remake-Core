using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SubterfugeCore.Models.GameEvents;

namespace SubterfugeServerConsole.Connections.Collections;

public class ChatMessageCollection : IDatabaseCollection<ChatMessage>
{
    private IMongoCollection<ChatMessage> _collection;

    public ChatMessageCollection(IMongoCollection<ChatMessage> collection)
    {
        this._collection = collection;
    }

    public async Task Upsert(ChatMessage item)
    {
        await _collection.ReplaceOneAsync(
            it => it.Id == item.Id,
            item,
            new UpdateOptions { IsUpsert = true }
        );
    }

    public async Task Delete(ChatMessage item)
    {
        await _collection.DeleteOneAsync(it => it.Id == item.Id);
    }

    public IMongoQueryable<ChatMessage> Query()
    {
        return _collection.AsQueryable();
    }

    public IEnumerable<CreateIndexModel<ChatMessage>> GetIndexes()
    {
        return new List<CreateIndexModel<ChatMessage>>()
        {
            new CreateIndexModel<ChatMessage>(Builders<ChatMessage>.IndexKeys.Ascending(message => message.RoomId)),
            new CreateIndexModel<ChatMessage>(Builders<ChatMessage>.IndexKeys.Ascending(message => message.GroupId)),
            new CreateIndexModel<ChatMessage>(Builders<ChatMessage>.IndexKeys.Ascending(message => message.SentBy)),
            new CreateIndexModel<ChatMessage>(Builders<ChatMessage>.IndexKeys.Ascending(message => message.UnixTimeCreatedAt)),
            new CreateIndexModel<ChatMessage>(Builders<ChatMessage>.IndexKeys.Text(message => message.Message)),
        };
    }

    public void Flush()
    {
        _collection.DeleteMany(FilterDefinition<ChatMessage>.Empty);
    }
}