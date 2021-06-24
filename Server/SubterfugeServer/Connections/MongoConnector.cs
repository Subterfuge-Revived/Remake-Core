using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using SubterfugeRemakeService;

namespace SubterfugeServerConsole.Connections
{
    public class MongoConnector
    {
        private static IMongoDatabase _database;
        private static bool _allowAdmin;
        
        public MongoConnector(string hostname, int port, bool allowAdmin)
        {
            _allowAdmin = allowAdmin;
            
            string username = "user";
            string password = "password";
            string mongoDbAuthMechanism = "SCRAM-SHA-1";
            MongoInternalIdentity internalIdentity = 
                new MongoInternalIdentity("admin", username);
            PasswordEvidence passwordEvidence = new PasswordEvidence(password);
            MongoCredential mongoCredential = 
                new MongoCredential(mongoDbAuthMechanism, 
                    internalIdentity, passwordEvidence);

            MongoClientSettings settings = new MongoClientSettings();
            settings.Server = new MongoServerAddress(hostname, port);
            settings.Credential = mongoCredential;
            settings.ApplicationName = "SubterfugeServer";
            
            var client = new MongoClient(settings);
            _database = client.GetDatabase("subterfugeDb");
            Console.WriteLine("Connected to database. Creating indexes...");
            CreateIndexes();
        }

        private async void CreateIndexes()
        {
            // Index Users
            Console.WriteLine("Indexing Users");
            BsonClassMap.RegisterClassMap<UserModel>(model =>
            {
                model.MapProperty(x => x.Claims);
                model.MapProperty(x => x.Id);
                model.MapProperty(x => x.Email);
                model.MapProperty(x => x.EmailVerified);
                model.MapProperty(x => x.Username);
                model.MapProperty(x => x.DeviceIdentifier);
                model.MapProperty(x => x.PasswordHash);
                model.MapProperty(x => x.PushNotificationIdentifier);
            });
            await GetUserCollection().Indexes.CreateOneAsync(new CreateIndexModel<UserModel>(Builders<UserModel>.IndexKeys.Ascending(user => user.Id)));
            await GetUserCollection().Indexes.CreateOneAsync(new CreateIndexModel<UserModel>(Builders<UserModel>.IndexKeys.Ascending(user => user.DeviceIdentifier)));
            await GetUserCollection().Indexes.CreateOneAsync(new CreateIndexModel<UserModel>(Builders<UserModel>.IndexKeys.Ascending(user => user.Username)));
            await GetUserCollection().Indexes.CreateOneAsync(new CreateIndexModel<UserModel>(Builders<UserModel>.IndexKeys.Ascending(user => user.Email)));

            // Index Game Rooms
            Console.WriteLine("Indexing Game Rooms");
            await GetGameRoomCollection().Indexes.CreateOneAsync(new CreateIndexModel<RoomModel>(Builders<RoomModel>.IndexKeys.Ascending(room => room.Id)));
            await GetGameRoomCollection().Indexes.CreateOneAsync(new CreateIndexModel<RoomModel>(Builders<RoomModel>.IndexKeys.Ascending(room => room.RoomName)));
            await GetGameRoomCollection().Indexes.CreateOneAsync(new CreateIndexModel<RoomModel>(Builders<RoomModel>.IndexKeys.Ascending(room => room.UnixTimeCreated)));
            
            // Index Friend relations
            Console.WriteLine("Indexing User Relations");
            await GetFriendCollection().Indexes.CreateOneAsync(new CreateIndexModel<FriendModel>(Builders<FriendModel>.IndexKeys.Ascending(relation => relation.PlayerId)));
            await GetFriendCollection().Indexes.CreateOneAsync(new CreateIndexModel<FriendModel>(Builders<FriendModel>.IndexKeys.Ascending(relation => relation.FriendId)));
            await GetFriendCollection().Indexes.CreateOneAsync(new CreateIndexModel<FriendModel>(Builders<FriendModel>.IndexKeys.Ascending(relation => relation.FriendStatus)));

            // Index Game Events
            Console.WriteLine("Indexing Game Events");
            await GetGameEventCollection().Indexes.CreateOneAsync(new CreateIndexModel<GameEventModel>(Builders<GameEventModel>.IndexKeys.Ascending(gameEvent => gameEvent.Id)));
            await GetGameEventCollection().Indexes.CreateOneAsync(new CreateIndexModel<GameEventModel>(Builders<GameEventModel>.IndexKeys.Ascending(gameEvent => gameEvent.IssuedBy)));
            await GetGameEventCollection().Indexes.CreateOneAsync(new CreateIndexModel<GameEventModel>(Builders<GameEventModel>.IndexKeys.Ascending(gameEvent => gameEvent.UnixTimeIssued)));
            await GetGameEventCollection().Indexes.CreateOneAsync(new CreateIndexModel<GameEventModel>(Builders<GameEventModel>.IndexKeys.Ascending(gameEvent => gameEvent.OccursAtTick)));
            await GetGameEventCollection().Indexes.CreateOneAsync(new CreateIndexModel<GameEventModel>(Builders<GameEventModel>.IndexKeys.Ascending(gameEvent => gameEvent.EventType)));

            // Index group chats.
            Console.WriteLine("Indexing Group chats");
            await GetMessagesCollection().Indexes.CreateOneAsync(new CreateIndexModel<MessageModel>(Builders<MessageModel>.IndexKeys.Ascending(message => message.RoomId)));
            await GetMessagesCollection().Indexes.CreateOneAsync(new CreateIndexModel<MessageModel>(Builders<MessageModel>.IndexKeys.Ascending(message => message.GroupId)));
            await GetMessagesCollection().Indexes.CreateOneAsync(new CreateIndexModel<MessageModel>(Builders<MessageModel>.IndexKeys.Ascending(message => message.SenderId)));
            await GetMessagesCollection().Indexes.CreateOneAsync(new CreateIndexModel<MessageModel>(Builders<MessageModel>.IndexKeys.Ascending(message => message.UnixTimeCreatedAt)));
            await GetMessagesCollection().Indexes.CreateOneAsync(new CreateIndexModel<MessageModel>(Builders<MessageModel>.IndexKeys.Text(message => message.Message)));
            
            // Index message groups
            Console.WriteLine("Indexing Messages");
            await GetMessageGroupCollection().Indexes.CreateOneAsync(new CreateIndexModel<GroupModel>(Builders<GroupModel>.IndexKeys.Ascending(group => group.Id)));
            await GetMessageGroupCollection().Indexes.CreateOneAsync(new CreateIndexModel<GroupModel>(Builders<GroupModel>.IndexKeys.Ascending(group => group.RoomId)));

            Console.WriteLine("Indexes created.");
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
                Console.WriteLine("Flushing database!");
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