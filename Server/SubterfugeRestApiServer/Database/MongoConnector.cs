using MongoDB.Driver;
using SubterfugeCore.Models.GameEvents;
using SubterfugeRestApiServer.Authentication;
using SubterfugeServerConsole.Connections.Models;

namespace SubterfugeServerConsole.Connections
{
    public class MongoConnector
    {
        private static IMongoDatabase _database;
        private ILogger _logger;
        private MongoConfiguration _config;
        
        public MongoConnector(MongoConfiguration config, ILogger logger)
        {
            _logger = logger;
            _config = config;
            
            _logger.LogInformation("Configuring MongoDB...");
            
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
            settings.Server = new MongoServerAddress(config.Host, config.Port);
            settings.Credential = mongoCredential;
            settings.ApplicationName = "SubterfugeGrpcServer";
            
            var client = new MongoClient(settings);
            _database = client.GetDatabase("subterfugeDb");
            logger.LogInformation("Connected to database. Creating indexes...");
            CreateIndexes();
            // Flush database is running in admin mode
            if (config.FlushDatabase)
            {
                FlushCollections();
            }

            if (config.CreateSuperUser)
            {
                CreateSuperUser();
            }
        }

        private async void CreateIndexes()
        {
            // Index Users
            _logger.LogInformation("Indexing Users");
            await GetUserCollection().Indexes.CreateOneAsync(new CreateIndexModel<UserModel>(Builders<UserModel>.IndexKeys.Ascending(user => user.Id)));
            await GetUserCollection().Indexes.CreateOneAsync(new CreateIndexModel<UserModel>(Builders<UserModel>.IndexKeys.Ascending(user => user.DeviceIdentifier)));
            await GetUserCollection().Indexes.CreateOneAsync(new CreateIndexModel<UserModel>(Builders<UserModel>.IndexKeys.Ascending(user => user.Username)));
            await GetUserCollection().Indexes.CreateOneAsync(new CreateIndexModel<UserModel>(Builders<UserModel>.IndexKeys.Ascending(user => user.Email)));
            
            // Index User IP Address Link
            _logger.LogInformation("Indexing User-Ip Links");
            await GetUserIpCollection().Indexes.CreateOneAsync(new CreateIndexModel<UserIpAddressLink>(Builders<UserIpAddressLink>.IndexKeys.Ascending(user => user.UserId)));
            await GetUserIpCollection().Indexes.CreateOneAsync(new CreateIndexModel<UserIpAddressLink>(Builders<UserIpAddressLink>.IndexKeys.Ascending(user => user.IpAddress)));

            // Index Game Rooms
            _logger.LogInformation("Indexing Game Rooms");
            await GetGameRoomCollection().Indexes.CreateOneAsync(new CreateIndexModel<GameConfiguration>(Builders<GameConfiguration>.IndexKeys.Ascending(room => room.Id)));
            await GetGameRoomCollection().Indexes.CreateOneAsync(new CreateIndexModel<GameConfiguration>(Builders<GameConfiguration>.IndexKeys.Ascending(room => room.RoomName)));
            await GetGameRoomCollection().Indexes.CreateOneAsync(new CreateIndexModel<GameConfiguration>(Builders<GameConfiguration>.IndexKeys.Ascending(room => room.UnixTimeCreated)));
            
            // Index Friend relations
            _logger.LogInformation("Indexing User Relations");
            await GetFriendCollection().Indexes.CreateOneAsync(new CreateIndexModel<Friend>(Builders<Friend>.IndexKeys.Ascending(relation => relation.PlayerId)));
            await GetFriendCollection().Indexes.CreateOneAsync(new CreateIndexModel<Friend>(Builders<Friend>.IndexKeys.Ascending(relation => relation.FriendId)));
            await GetFriendCollection().Indexes.CreateOneAsync(new CreateIndexModel<Friend>(Builders<Friend>.IndexKeys.Ascending(relation => relation.RelationshipStatus)));

            // Index Game Events
            _logger.LogInformation("Indexing Game Events");
            await GetGameEventCollection().Indexes.CreateOneAsync(new CreateIndexModel<GameEventData>(Builders<GameEventData>.IndexKeys.Ascending(gameEvent => gameEvent.Id)));
            await GetGameEventCollection().Indexes.CreateOneAsync(new CreateIndexModel<GameEventData>(Builders<GameEventData>.IndexKeys.Ascending(gameEvent => gameEvent.IssuedBy)));
            await GetGameEventCollection().Indexes.CreateOneAsync(new CreateIndexModel<GameEventData>(Builders<GameEventData>.IndexKeys.Ascending(gameEvent => gameEvent.UnixTimeIssued)));
            await GetGameEventCollection().Indexes.CreateOneAsync(new CreateIndexModel<GameEventData>(Builders<GameEventData>.IndexKeys.Ascending(gameEvent => gameEvent.OccursAtTick)));

            // Index group chats.
            _logger.LogInformation("Indexing Group chats");
            await GetMessagesCollection().Indexes.CreateOneAsync(new CreateIndexModel<ChatMessage>(Builders<ChatMessage>.IndexKeys.Ascending(message => message.RoomId)));
            await GetMessagesCollection().Indexes.CreateOneAsync(new CreateIndexModel<ChatMessage>(Builders<ChatMessage>.IndexKeys.Ascending(message => message.GroupId)));
            await GetMessagesCollection().Indexes.CreateOneAsync(new CreateIndexModel<ChatMessage>(Builders<ChatMessage>.IndexKeys.Ascending(message => message.SentBy)));
            await GetMessagesCollection().Indexes.CreateOneAsync(new CreateIndexModel<ChatMessage>(Builders<ChatMessage>.IndexKeys.Ascending(message => message.UnixTimeCreatedAt)));
            await GetMessagesCollection().Indexes.CreateOneAsync(new CreateIndexModel<ChatMessage>(Builders<ChatMessage>.IndexKeys.Text(message => message.Message)));
            
            // Index message groups
            _logger.LogInformation("Indexing Messages");
            await GetMessageGroupCollection().Indexes.CreateOneAsync(new CreateIndexModel<MessageGroupDatabaseModel>(Builders<MessageGroupDatabaseModel>.IndexKeys.Ascending(group => group.Id)));
            await GetMessageGroupCollection().Indexes.CreateOneAsync(new CreateIndexModel<MessageGroupDatabaseModel>(Builders<MessageGroupDatabaseModel>.IndexKeys.Ascending(group => group.RoomId)));
            
            // Index Specialist Configurations
            _logger.LogInformation("Indexing Specialist Configurations");
            await GetSpecialistCollection().Indexes.CreateOneAsync(new CreateIndexModel<SpecialistConfiguration>(Builders<SpecialistConfiguration>.IndexKeys.Ascending(spec => spec.Id)));
            await GetSpecialistCollection().Indexes.CreateOneAsync(new CreateIndexModel<SpecialistConfiguration>(Builders<SpecialistConfiguration>.IndexKeys.Ascending(spec => spec.Creator.Id)));
            await GetSpecialistCollection().Indexes.CreateOneAsync(new CreateIndexModel<SpecialistConfiguration>(Builders<SpecialistConfiguration>.IndexKeys.Ascending(spec => spec.PromotesFromSpecialistId)));
            await GetSpecialistCollection().Indexes.CreateOneAsync(new CreateIndexModel<SpecialistConfiguration>(Builders<SpecialistConfiguration>.IndexKeys.Text(spec => spec.SpecialistName)));
            
            // Index Specialist Configurations
            _logger.LogInformation("Indexing Specialist Packages");
            await GetSpecialistPackageCollection().Indexes.CreateOneAsync(new CreateIndexModel<SpecialistPackage>(Builders<SpecialistPackage>.IndexKeys.Ascending(package => package.Id)));
            await GetSpecialistPackageCollection().Indexes.CreateOneAsync(new CreateIndexModel<SpecialistPackage>(Builders<SpecialistPackage>.IndexKeys.Ascending(package => package.Creator.Id)));
            await GetSpecialistPackageCollection().Indexes.CreateOneAsync(new CreateIndexModel<SpecialistPackage>(Builders<SpecialistPackage>.IndexKeys.Text(package => package.PackageName)));

            _logger.LogInformation("Indexes created.");
        }

        public static IMongoCollection<UserIpAddressLink> GetUserIpCollection()
        {
            return _database.GetCollection<UserIpAddressLink>("UserIpAddressLink");
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

        public void FlushCollections()
        {
            _logger.LogInformation("Flushing database!");
            GetUserCollection().DeleteMany(FilterDefinition<UserModel>.Empty);
            GetUserIpCollection().DeleteMany(FilterDefinition<UserIpAddressLink>.Empty);
            GetGameRoomCollection().DeleteMany(FilterDefinition<GameConfiguration>.Empty);
            GetFriendCollection().DeleteMany(FilterDefinition<Friend>.Empty);
            GetGameEventCollection().DeleteMany(FilterDefinition<GameEventData>.Empty);
            GetMessagesCollection().DeleteMany(FilterDefinition<ChatMessage>.Empty);
            GetMessageGroupCollection().DeleteMany(FilterDefinition<MessageGroupDatabaseModel>.Empty);
            GetSpecialistCollection().DeleteMany(FilterDefinition<SpecialistConfiguration>.Empty);
            GetSpecialistPackageCollection().DeleteMany(FilterDefinition<SpecialistPackage>.Empty);
        }

        public async void CreateSuperUser()
        {
            DbUserModel dbUserModel = new DbUserModel(new UserModel()
            {
                Id = "1",
                Username = _config.SuperUserUsername,
                Email = _config.SuperUserUsername,
                EmailVerified = true,
                PasswordHash = JwtManager.HashPassword(_config.SuperUserPassword),
                Claims = new [] { UserClaim.User, UserClaim.Administrator, UserClaim.EmailVerified }
            });
            try
            {
                await dbUserModel.SaveToDatabase();
                _logger.LogInformation("SuperUser Username: {username}", _config.SuperUserUsername);
                _logger.LogInformation("SuperUser Password: {password}", _config.SuperUserPassword);
            }
            catch (MongoWriteException writeException)
            {
                _logger.LogWarning("Admin user already exists. Did not re-create. The configured credentials may be incorrect");
                _logger.LogInformation("SuperUser Username: {username}", _config.SuperUserUsername);
                _logger.LogInformation("SuperUser Password: {password}", _config.SuperUserPassword);
            }
        }
        
    }
}