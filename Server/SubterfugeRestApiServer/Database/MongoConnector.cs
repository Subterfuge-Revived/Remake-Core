using MongoDB.Driver;
using SubterfugeCore.Models.GameEvents;

namespace SubterfugeServerConsole.Connections
{
    public class MongoConnector
    {
        private static IMongoDatabase _database;
        private static bool _allowAdmin;
        
        public MongoConnector(string hostname, int port, bool allowAdmin)
        {
            System.Diagnostics.Debug.WriteLine("Configuring MongoDB...");
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
            settings.ApplicationName = "SubterfugeGrpcServer";
            
            var client = new MongoClient(settings);
            _database = client.GetDatabase("subterfugeDb");
            System.Diagnostics.Debug.WriteLine("Connected to database. Creating indexes...");
            CreateIndexes();
        }

        private async void CreateIndexes()
        {
            // Index Users
            System.Diagnostics.Debug.WriteLine("Indexing Users");
            await GetUserCollection().Indexes.CreateOneAsync(new CreateIndexModel<UserModel>(Builders<UserModel>.IndexKeys.Ascending(user => user.Id)));
            await GetUserCollection().Indexes.CreateOneAsync(new CreateIndexModel<UserModel>(Builders<UserModel>.IndexKeys.Ascending(user => user.DeviceIdentifier)));
            await GetUserCollection().Indexes.CreateOneAsync(new CreateIndexModel<UserModel>(Builders<UserModel>.IndexKeys.Ascending(user => user.Username)));
            await GetUserCollection().Indexes.CreateOneAsync(new CreateIndexModel<UserModel>(Builders<UserModel>.IndexKeys.Ascending(user => user.Email)));

            // Index Game Rooms
            System.Diagnostics.Debug.WriteLine("Indexing Game Rooms");
            await GetGameRoomCollection().Indexes.CreateOneAsync(new CreateIndexModel<GameConfiguration>(Builders<GameConfiguration>.IndexKeys.Ascending(room => room.Id)));
            await GetGameRoomCollection().Indexes.CreateOneAsync(new CreateIndexModel<GameConfiguration>(Builders<GameConfiguration>.IndexKeys.Ascending(room => room.RoomName)));
            await GetGameRoomCollection().Indexes.CreateOneAsync(new CreateIndexModel<GameConfiguration>(Builders<GameConfiguration>.IndexKeys.Ascending(room => room.UnixTimeCreated)));
            
            // Index Friend relations
            System.Diagnostics.Debug.WriteLine("Indexing User Relations");
            await GetFriendCollection().Indexes.CreateOneAsync(new CreateIndexModel<Friend>(Builders<Friend>.IndexKeys.Ascending(relation => relation.PlayerId)));
            await GetFriendCollection().Indexes.CreateOneAsync(new CreateIndexModel<Friend>(Builders<Friend>.IndexKeys.Ascending(relation => relation.FriendId)));
            await GetFriendCollection().Indexes.CreateOneAsync(new CreateIndexModel<Friend>(Builders<Friend>.IndexKeys.Ascending(relation => relation.RelationshipStatus)));

            // Index Game Events
            System.Diagnostics.Debug.WriteLine("Indexing Game Events");
            await GetGameEventCollection().Indexes.CreateOneAsync(new CreateIndexModel<GameEventData>(Builders<GameEventData>.IndexKeys.Ascending(gameEvent => gameEvent.Id)));
            await GetGameEventCollection().Indexes.CreateOneAsync(new CreateIndexModel<GameEventData>(Builders<GameEventData>.IndexKeys.Ascending(gameEvent => gameEvent.IssuedBy)));
            await GetGameEventCollection().Indexes.CreateOneAsync(new CreateIndexModel<GameEventData>(Builders<GameEventData>.IndexKeys.Ascending(gameEvent => gameEvent.UnixTimeIssued)));
            await GetGameEventCollection().Indexes.CreateOneAsync(new CreateIndexModel<GameEventData>(Builders<GameEventData>.IndexKeys.Ascending(gameEvent => gameEvent.OccursAtTick)));

            // Index group chats.
            System.Diagnostics.Debug.WriteLine("Indexing Group chats");
            await GetMessagesCollection().Indexes.CreateOneAsync(new CreateIndexModel<ChatMessage>(Builders<ChatMessage>.IndexKeys.Ascending(message => message.RoomId)));
            await GetMessagesCollection().Indexes.CreateOneAsync(new CreateIndexModel<ChatMessage>(Builders<ChatMessage>.IndexKeys.Ascending(message => message.GroupId)));
            await GetMessagesCollection().Indexes.CreateOneAsync(new CreateIndexModel<ChatMessage>(Builders<ChatMessage>.IndexKeys.Ascending(message => message.SentBy)));
            await GetMessagesCollection().Indexes.CreateOneAsync(new CreateIndexModel<ChatMessage>(Builders<ChatMessage>.IndexKeys.Ascending(message => message.UnixTimeCreatedAt)));
            await GetMessagesCollection().Indexes.CreateOneAsync(new CreateIndexModel<ChatMessage>(Builders<ChatMessage>.IndexKeys.Text(message => message.Message)));
            
            // Index message groups
            System.Diagnostics.Debug.WriteLine("Indexing Messages");
            await GetMessageGroupCollection().Indexes.CreateOneAsync(new CreateIndexModel<MessageGroupDatabaseModel>(Builders<MessageGroupDatabaseModel>.IndexKeys.Ascending(group => group.Id)));
            await GetMessageGroupCollection().Indexes.CreateOneAsync(new CreateIndexModel<MessageGroupDatabaseModel>(Builders<MessageGroupDatabaseModel>.IndexKeys.Ascending(group => group.RoomId)));
            
            // Index Specialist Configurations
            System.Diagnostics.Debug.WriteLine("Indexing Specialist Configurations");
            await GetSpecialistCollection().Indexes.CreateOneAsync(new CreateIndexModel<SpecialistConfiguration>(Builders<SpecialistConfiguration>.IndexKeys.Ascending(spec => spec.Id)));
            await GetSpecialistCollection().Indexes.CreateOneAsync(new CreateIndexModel<SpecialistConfiguration>(Builders<SpecialistConfiguration>.IndexKeys.Ascending(spec => spec.Creator.Id)));
            await GetSpecialistCollection().Indexes.CreateOneAsync(new CreateIndexModel<SpecialistConfiguration>(Builders<SpecialistConfiguration>.IndexKeys.Ascending(spec => spec.PromotesFromSpecialistId)));
            await GetSpecialistCollection().Indexes.CreateOneAsync(new CreateIndexModel<SpecialistConfiguration>(Builders<SpecialistConfiguration>.IndexKeys.Text(spec => spec.SpecialistName)));
            
            // Index Specialist Configurations
            System.Diagnostics.Debug.WriteLine("Indexing Specialist Packages");
            await GetSpecialistPackageCollection().Indexes.CreateOneAsync(new CreateIndexModel<SpecialistPackage>(Builders<SpecialistPackage>.IndexKeys.Ascending(package => package.Id)));
            await GetSpecialistPackageCollection().Indexes.CreateOneAsync(new CreateIndexModel<SpecialistPackage>(Builders<SpecialistPackage>.IndexKeys.Ascending(package => package.Creator.Id)));
            await GetSpecialistPackageCollection().Indexes.CreateOneAsync(new CreateIndexModel<SpecialistPackage>(Builders<SpecialistPackage>.IndexKeys.Text(package => package.PackageName)));

            System.Diagnostics.Debug.WriteLine("Indexes created.");
        }

        public static IMongoCollection<UserModel> GetUserCollection()
        {
            return _database.GetCollection<UserModel>("Users");
        }
        
        public static IMongoCollection<GameConfiguration> GetGameRoomCollection()
        {
            return _database.GetCollection<GameConfiguration>("GameRooms");
        }
        
        public static IMongoCollection<Friend> GetFriendCollection()
        {
            return _database.GetCollection<Friend>("Friends");
        }

        public static IMongoCollection<GameEventData> GetGameEventCollection()
        {
            return _database.GetCollection<GameEventData>("GameEvents");
        }

        public static IMongoCollection<ChatMessage> GetMessagesCollection()
        {
            return _database.GetCollection<ChatMessage>("Messages");
        }
        
        public static IMongoCollection<MessageGroupDatabaseModel> GetMessageGroupCollection()
        {
            return _database.GetCollection<MessageGroupDatabaseModel>("MessageGroups");
        }
        
        public static IMongoCollection<SpecialistConfiguration> GetSpecialistCollection()
        {
            return _database.GetCollection<SpecialistConfiguration>("Specialists");
        }

        public static IMongoCollection<SpecialistPackage> GetSpecialistPackageCollection()
        {
            return _database.GetCollection<SpecialistPackage>("SpecialistPackages");
        }

        public static void FlushCollections()
        {
            if (_allowAdmin)
            {
                System.Diagnostics.Debug.WriteLine("Flushing database!");
                GetUserCollection().DeleteMany(FilterDefinition<UserModel>.Empty);
                GetGameRoomCollection().DeleteMany(FilterDefinition<GameConfiguration>.Empty);
                GetFriendCollection().DeleteMany(FilterDefinition<Friend>.Empty);
                GetGameEventCollection().DeleteMany(FilterDefinition<GameEventData>.Empty);
                GetMessagesCollection().DeleteMany(FilterDefinition<ChatMessage>.Empty);
                GetMessageGroupCollection().DeleteMany(FilterDefinition<MessageGroupDatabaseModel>.Empty);
                GetSpecialistCollection().DeleteMany(FilterDefinition<SpecialistConfiguration>.Empty);
                GetSpecialistPackageCollection().DeleteMany(FilterDefinition<SpecialistPackage>.Empty);
            }
        }
        
    }
}