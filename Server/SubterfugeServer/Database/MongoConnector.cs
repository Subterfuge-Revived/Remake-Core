using System;
using System.Collections.Generic;
using System.Linq;
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
            await GetUserCollection().Indexes.CreateOneAsync(new CreateIndexModel<UserModelMapper>(Builders<UserModelMapper>.IndexKeys.Ascending(user => user.Id)));
            await GetUserCollection().Indexes.CreateOneAsync(new CreateIndexModel<UserModelMapper>(Builders<UserModelMapper>.IndexKeys.Ascending(user => user.DeviceIdentifier)));
            await GetUserCollection().Indexes.CreateOneAsync(new CreateIndexModel<UserModelMapper>(Builders<UserModelMapper>.IndexKeys.Ascending(user => user.Username)));
            await GetUserCollection().Indexes.CreateOneAsync(new CreateIndexModel<UserModelMapper>(Builders<UserModelMapper>.IndexKeys.Ascending(user => user.Email)));

            // Index Game Rooms
            Console.WriteLine("Indexing Game Rooms");
            await GetGameRoomCollection().Indexes.CreateOneAsync(new CreateIndexModel<RoomModelMapper>(Builders<RoomModelMapper>.IndexKeys.Ascending(room => room.Id)));
            await GetGameRoomCollection().Indexes.CreateOneAsync(new CreateIndexModel<RoomModelMapper>(Builders<RoomModelMapper>.IndexKeys.Ascending(room => room.RoomName)));
            await GetGameRoomCollection().Indexes.CreateOneAsync(new CreateIndexModel<RoomModelMapper>(Builders<RoomModelMapper>.IndexKeys.Ascending(room => room.UnixTimeCreated)));
            
            // Index Friend relations
            Console.WriteLine("Indexing User Relations");
            await GetFriendCollection().Indexes.CreateOneAsync(new CreateIndexModel<FriendModel>(Builders<FriendModel>.IndexKeys.Ascending(relation => relation.PlayerId)));
            await GetFriendCollection().Indexes.CreateOneAsync(new CreateIndexModel<FriendModel>(Builders<FriendModel>.IndexKeys.Ascending(relation => relation.FriendId)));
            await GetFriendCollection().Indexes.CreateOneAsync(new CreateIndexModel<FriendModel>(Builders<FriendModel>.IndexKeys.Ascending(relation => relation.FriendStatus)));

            // Index Game Events
            Console.WriteLine("Indexing Game Events");
            await GetGameEventCollection().Indexes.CreateOneAsync(new CreateIndexModel<GameEventModelMapper>(Builders<GameEventModelMapper>.IndexKeys.Ascending(gameEvent => gameEvent.Id)));
            await GetGameEventCollection().Indexes.CreateOneAsync(new CreateIndexModel<GameEventModelMapper>(Builders<GameEventModelMapper>.IndexKeys.Ascending(gameEvent => gameEvent.IssuedBy)));
            await GetGameEventCollection().Indexes.CreateOneAsync(new CreateIndexModel<GameEventModelMapper>(Builders<GameEventModelMapper>.IndexKeys.Ascending(gameEvent => gameEvent.UnixTimeIssued)));
            await GetGameEventCollection().Indexes.CreateOneAsync(new CreateIndexModel<GameEventModelMapper>(Builders<GameEventModelMapper>.IndexKeys.Ascending(gameEvent => gameEvent.OccursAtTick)));
            await GetGameEventCollection().Indexes.CreateOneAsync(new CreateIndexModel<GameEventModelMapper>(Builders<GameEventModelMapper>.IndexKeys.Ascending(gameEvent => gameEvent.EventType)));

            // Index group chats.
            Console.WriteLine("Indexing Group chats");
            await GetMessagesCollection().Indexes.CreateOneAsync(new CreateIndexModel<MessageModel>(Builders<MessageModel>.IndexKeys.Ascending(message => message.RoomId)));
            await GetMessagesCollection().Indexes.CreateOneAsync(new CreateIndexModel<MessageModel>(Builders<MessageModel>.IndexKeys.Ascending(message => message.GroupId)));
            await GetMessagesCollection().Indexes.CreateOneAsync(new CreateIndexModel<MessageModel>(Builders<MessageModel>.IndexKeys.Ascending(message => message.SenderId)));
            await GetMessagesCollection().Indexes.CreateOneAsync(new CreateIndexModel<MessageModel>(Builders<MessageModel>.IndexKeys.Ascending(message => message.UnixTimeCreatedAt)));
            await GetMessagesCollection().Indexes.CreateOneAsync(new CreateIndexModel<MessageModel>(Builders<MessageModel>.IndexKeys.Text(message => message.Message)));
            
            // Index message groups
            Console.WriteLine("Indexing Messages");
            await GetMessageGroupCollection().Indexes.CreateOneAsync(new CreateIndexModel<GroupModelMapper>(Builders<GroupModelMapper>.IndexKeys.Ascending(group => group.Id)));
            await GetMessageGroupCollection().Indexes.CreateOneAsync(new CreateIndexModel<GroupModelMapper>(Builders<GroupModelMapper>.IndexKeys.Ascending(group => group.RoomId)));

            Console.WriteLine("Indexes created.");
        }

        public static IMongoCollection<UserModelMapper> GetUserCollection()
        {
            return _database.GetCollection<UserModelMapper>("Users");
        }
        
        public static IMongoCollection<RoomModelMapper> GetGameRoomCollection()
        {
            return _database.GetCollection<RoomModelMapper>("GameRooms");
        }
        
        public static IMongoCollection<FriendModel> GetFriendCollection()
        {
            return _database.GetCollection<FriendModel>("Friends");
        }

        public static IMongoCollection<GameEventModelMapper> GetGameEventCollection()
        {
            return _database.GetCollection<GameEventModelMapper>("GameEvents");
        }

        public static IMongoCollection<MessageModel> GetMessagesCollection()
        {
            return _database.GetCollection<MessageModel>("Messages");
        }
        
        public static IMongoCollection<GroupModelMapper> GetMessageGroupCollection()
        {
            return _database.GetCollection<GroupModelMapper>("MessageGroups");
        }

        public static void FlushCollections()
        {
            if (_allowAdmin)
            {
                Console.WriteLine("Flushing database!");
                GetUserCollection().DeleteMany(FilterDefinition<UserModelMapper>.Empty);
                GetGameRoomCollection().DeleteMany(FilterDefinition<RoomModelMapper>.Empty);
                GetFriendCollection().DeleteMany(FilterDefinition<FriendModel>.Empty);
                GetGameEventCollection().DeleteMany(FilterDefinition<GameEventModelMapper>.Empty);
                GetMessagesCollection().DeleteMany(FilterDefinition<MessageModel>.Empty);
                GetMessageGroupCollection().DeleteMany(FilterDefinition<GroupModelMapper>.Empty);
            }
        }
        
    }
}