using MongoDB.Bson;
using MongoDB.Driver;
using SubterfugeRemakeService;

namespace SubterfugeServerConsole.Connections
{
    public class MongoConnector
    {
        private static IMongoDatabase _database;
        private static bool _allowAdmin;
        
        public MongoConnector(string hostname, string port, bool allowAdmin)
        {
            MongoConnector._allowAdmin = allowAdmin;
            MongoClientSettings settings = new MongoClientSettings();
            settings.Credential = MongoCredential.CreateCredential("subterfugeDb", "user", "pass");
            settings.Server = MongoServerAddress.Parse($"{hostname}:{port}");
            settings.ApplicationName = "SubterfugeServer";
            
            var client = new MongoClient(settings);
            _database = client.GetDatabase("subterfugeDb");
            CreateIndexes();
        }

        private async void CreateIndexes()
        {
            // Index Users
            await GetUserCollection().Indexes.CreateOneAsync(new CreateIndexModel<UserModel>(Builders<UserModel>.IndexKeys.Ascending(user => user.Id)));
            await GetUserCollection().Indexes.CreateOneAsync(new CreateIndexModel<UserModel>(Builders<UserModel>.IndexKeys.Ascending(user => user.DeviceIdentifier)));
            await GetUserCollection().Indexes.CreateOneAsync(new CreateIndexModel<UserModel>(Builders<UserModel>.IndexKeys.Ascending(user => user.Username)));
            await GetUserCollection().Indexes.CreateOneAsync(new CreateIndexModel<UserModel>(Builders<UserModel>.IndexKeys.Ascending(user => user.Email)));

            // Index Game Rooms
            await GetGameRoomCollection().Indexes.CreateOneAsync(new CreateIndexModel<RoomModel>(Builders<RoomModel>.IndexKeys.Ascending(room => room.RoomId)));
            await GetGameRoomCollection().Indexes.CreateOneAsync(new CreateIndexModel<RoomModel>(Builders<RoomModel>.IndexKeys.Ascending(room => room.RoomName)));
            await GetGameRoomCollection().Indexes.CreateOneAsync(new CreateIndexModel<RoomModel>(Builders<RoomModel>.IndexKeys.Ascending(room => room.UnixTimeCreated)));
            
            // Index Friend relations
            await GetFriendCollection().Indexes.CreateOneAsync(new CreateIndexModel<FriendModel>(Builders<FriendModel>.IndexKeys.Ascending(relation => relation.PlayerId)));
            await GetFriendCollection().Indexes.CreateOneAsync(new CreateIndexModel<FriendModel>(Builders<FriendModel>.IndexKeys.Ascending(relation => relation.FriendId)));
            await GetFriendCollection().Indexes.CreateOneAsync(new CreateIndexModel<FriendModel>(Builders<FriendModel>.IndexKeys.Ascending(relation => relation.FriendStatus)));

            // Index Game Events
            await GetGameEventCollection().Indexes.CreateOneAsync(new CreateIndexModel<GameEventModel>(Builders<GameEventModel>.IndexKeys.Ascending(gameEvent => gameEvent.EventId)));
            await GetGameEventCollection().Indexes.CreateOneAsync(new CreateIndexModel<GameEventModel>(Builders<GameEventModel>.IndexKeys.Ascending(gameEvent => gameEvent.IssuedBy)));
            await GetGameEventCollection().Indexes.CreateOneAsync(new CreateIndexModel<GameEventModel>(Builders<GameEventModel>.IndexKeys.Ascending(gameEvent => gameEvent.UnixTimeIssued)));
            await GetGameEventCollection().Indexes.CreateOneAsync(new CreateIndexModel<GameEventModel>(Builders<GameEventModel>.IndexKeys.Ascending(gameEvent => gameEvent.OccursAtTick)));
            await GetGameEventCollection().Indexes.CreateOneAsync(new CreateIndexModel<GameEventModel>(Builders<GameEventModel>.IndexKeys.Ascending(gameEvent => gameEvent.EventType)));

            // Index group chats.
            await GetMessagesCollection().Indexes.CreateOneAsync(new CreateIndexModel<MessageModel>(Builders<MessageModel>.IndexKeys.Ascending(message => message.RoomId)));
            await GetMessagesCollection().Indexes.CreateOneAsync(new CreateIndexModel<MessageModel>(Builders<MessageModel>.IndexKeys.Ascending(message => message.GroupId)));
            await GetMessagesCollection().Indexes.CreateOneAsync(new CreateIndexModel<MessageModel>(Builders<MessageModel>.IndexKeys.Ascending(message => message.SenderId)));
            await GetMessagesCollection().Indexes.CreateOneAsync(new CreateIndexModel<MessageModel>(Builders<MessageModel>.IndexKeys.Ascending(message => message.UnixTimeCreatedAt)));
            await GetMessagesCollection().Indexes.CreateOneAsync(new CreateIndexModel<MessageModel>(Builders<MessageModel>.IndexKeys.Text(message => message.Message)));
            
            // Index message groups
            await GetMessageGroupCollection().Indexes.CreateOneAsync(new CreateIndexModel<GroupModel>(Builders<GroupModel>.IndexKeys.Ascending(group => group.GroupId)));
            await GetMessageGroupCollection().Indexes.CreateOneAsync(new CreateIndexModel<GroupModel>(Builders<GroupModel>.IndexKeys.Ascending(group => group.RoomId)));
        }

        public static IMongoCollection<UserModel> GetUserCollection()
        {
            return _database.GetCollection<UserModel>("Users");
        }
        
        public static IMongoCollection<RoomModel> GetGameRoomCollection()
        {
            return _database.GetCollection<RoomModel>("GameRooms");
        }
        
        public static IMongoCollection<FriendModel> GetFriendCollection()
        {
            return _database.GetCollection<FriendModel>("Friends");
        }

        public static IMongoCollection<GameEventModel> GetGameEventCollection()
        {
            return _database.GetCollection<GameEventModel>("GameEvents");
        }

        public static IMongoCollection<MessageModel> GetMessagesCollection()
        {
            return _database.GetCollection<MessageModel>("Messages");
        }
        
        public static IMongoCollection<GroupModel> GetMessageGroupCollection()
        {
            return _database.GetCollection<GroupModel>("MessageGroups");
        }

        public static void FlushCollections()
        {
            if (_allowAdmin)
            {
                GetUserCollection().DeleteMany(FilterDefinition<UserModel>.Empty);
                GetGameRoomCollection().DeleteMany(FilterDefinition<RoomModel>.Empty);
                GetFriendCollection().DeleteMany(FilterDefinition<FriendModel>.Empty);
                GetGameEventCollection().DeleteMany(FilterDefinition<GameEventModel>.Empty);
                GetMessagesCollection().DeleteMany(FilterDefinition<MessageModel>.Empty);
                GetMessageGroupCollection().DeleteMany(FilterDefinition<GroupModel>.Empty);
            }
        }
        
    }
}