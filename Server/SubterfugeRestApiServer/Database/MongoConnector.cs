using MongoDB.Driver;
using SubterfugeCore.Models;
using SubterfugeCore.Models.GameEvents;
using SubterfugeRestApiServer.Authentication;
using SubterfugeServerConsole.Connections.Collections;
using SubterfugeServerConsole.Connections.Models;

namespace SubterfugeServerConsole.Connections
{
    public class MongoConnector
    {
        private static IMongoDatabase _database;
        private ILogger _logger;
        private MongoConfiguration _config;

        public static IDatabaseCollection<UserModel> UserCollection;
        public static IDatabaseCollection<GameConfiguration> GameConfigurationCollection;
        public static IDatabaseCollection<Friend> FriendCollection;
        public static IDatabaseCollection<GameEventData> GameEventCollection;
        public static IDatabaseCollection<ChatMessage> ChatMessageCollection;
        public static IDatabaseCollection<MessageGroupDatabaseModel> GroupCollection;
        public static IDatabaseCollection<SpecialistConfiguration> SpecialistCollection;
        public static IDatabaseCollection<SpecialistPackage> SpecialistPackageCollection;
        public static IDatabaseCollection<ServerActionLog> ServerActionLogCollection;
        public static IDatabaseCollection<ServerExceptionLog> ServerExceptionLogCollection;

        public MongoConnector(MongoConfiguration config, ILogger logger)
        {
            _logger = logger;
            _config = config;
            
            _logger.LogInformation("Configuring MongoDB...");
            _database = ConnectToMongo();

            // Instantiate collection interfaces
            UserCollection = new UserModelCollection(GetCollection<UserModel>());
            GameConfigurationCollection = new GameConfigurationCollection(GetCollection<GameConfiguration>());
            FriendCollection = new FriendCollection(GetCollection<Friend>());
            GameEventCollection = new GameEventCollection(GetCollection<GameEventData>());
            ChatMessageCollection = new ChatMessageCollection(GetCollection<ChatMessage>());
            GroupCollection = new GroupCollection(GetCollection<MessageGroupDatabaseModel>());
            SpecialistCollection = new SpecialistConfigurationCollection(GetCollection<SpecialistConfiguration>());
            SpecialistPackageCollection = new SpecialistPackageCollection(GetCollection<SpecialistPackage>());
            ServerActionLogCollection = new ServerActionLogCollection(GetCollection<ServerActionLog>());
            ServerExceptionLogCollection = new ServerExceptionLogCollection(GetCollection<ServerExceptionLog>());
 
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

        private IMongoDatabase ConnectToMongo()
        {
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
            settings.Server = new MongoServerAddress(_config.Host, _config.Port);
            settings.Credential = mongoCredential;
            settings.ApplicationName = "SubterfugeServer";
            
            var client = new MongoClient(settings);
            return client.GetDatabase("subterfugeDb");
        }

        private async void CreateIndexes()
        {
            var indexTasks = new List<Task>()
            {
                GetCollection<UserModel>().Indexes.CreateManyAsync(UserCollection.GetIndexes()),
                GetCollection<GameConfiguration>().Indexes.CreateManyAsync(GameConfigurationCollection.GetIndexes()),
                GetCollection<Friend>().Indexes.CreateManyAsync(FriendCollection.GetIndexes()),
                GetCollection<GameEventData>().Indexes.CreateManyAsync(GameEventCollection.GetIndexes()),
                GetCollection<ChatMessage>().Indexes.CreateManyAsync(ChatMessageCollection.GetIndexes()),
                GetCollection<MessageGroupDatabaseModel>().Indexes.CreateManyAsync(GroupCollection.GetIndexes()),
                GetCollection<SpecialistConfiguration>().Indexes.CreateManyAsync(SpecialistCollection.GetIndexes()),
                GetCollection<SpecialistPackage>().Indexes.CreateManyAsync(SpecialistPackageCollection.GetIndexes()),
                GetCollection<ServerActionLog>().Indexes.CreateManyAsync(ServerActionLogCollection.GetIndexes()),
                GetCollection<ServerExceptionLog>().Indexes.CreateManyAsync(ServerExceptionLogCollection.GetIndexes()),
                
                // Index User IP Address Link
                GetCollection<UserIpAddressLink>().Indexes.CreateOneAsync(new CreateIndexModel<UserIpAddressLink>(Builders<UserIpAddressLink>.IndexKeys.Ascending(user => user.UserId))),
                GetCollection<UserIpAddressLink>().Indexes.CreateOneAsync(new CreateIndexModel<UserIpAddressLink>(Builders<UserIpAddressLink>.IndexKeys.Ascending(user => user.IpAddress))),
            };

            
            _logger.LogInformation("Creating database indexes");
            await Task.WhenAll(indexTasks);
            _logger.LogInformation("Database created.");
        }

        private IMongoCollection<T> GetCollection<T>()
        {
            return _database.GetCollection<T>(typeof(T).ToString());
        }

        public void FlushCollections()
        {
            _logger.LogInformation("Flushing database!");
            UserCollection.Flush();
            GameConfigurationCollection.Flush();
            FriendCollection.Flush();
            GameEventCollection.Flush();
            ChatMessageCollection.Flush();
            GroupCollection.Flush();
            SpecialistCollection.Flush();
            SpecialistPackageCollection.Flush();
            
            GetCollection<UserIpAddressLink>().DeleteMany(FilterDefinition<UserIpAddressLink>.Empty);
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