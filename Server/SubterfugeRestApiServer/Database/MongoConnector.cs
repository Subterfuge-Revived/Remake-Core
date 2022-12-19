using MongoDB.Driver;
using SubterfugeCore.Models;
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
            settings.ApplicationName = "SubterfugeServer";
            
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
            var indexTasks = new List<Task>()
            {
                // Index Users
                GetCollection<UserModel>().Indexes.CreateOneAsync(new CreateIndexModel<UserModel>(Builders<UserModel>.IndexKeys.Ascending(user => user.Id))),
                GetCollection<UserModel>().Indexes.CreateOneAsync(new CreateIndexModel<UserModel>(Builders<UserModel>.IndexKeys.Ascending(user => user.DeviceIdentifier))),
                GetCollection<UserModel>().Indexes.CreateOneAsync(new CreateIndexModel<UserModel>(Builders<UserModel>.IndexKeys.Ascending(user => user.Username))),
                GetCollection<UserModel>().Indexes.CreateOneAsync(new CreateIndexModel<UserModel>(Builders<UserModel>.IndexKeys.Ascending(user => user.Email))),
                
                // Index User IP Address Link
                GetCollection<UserIpAddressLink>().Indexes.CreateOneAsync(new CreateIndexModel<UserIpAddressLink>(Builders<UserIpAddressLink>.IndexKeys.Ascending(user => user.UserId))),
                GetCollection<UserIpAddressLink>().Indexes.CreateOneAsync(new CreateIndexModel<UserIpAddressLink>(Builders<UserIpAddressLink>.IndexKeys.Ascending(user => user.IpAddress))),
                
                
                // Index Game Rooms
                GetCollection<GameConfiguration>().Indexes.CreateOneAsync(new CreateIndexModel<GameConfiguration>(Builders<GameConfiguration>.IndexKeys.Ascending(room => room.Id))),
                GetCollection<GameConfiguration>().Indexes.CreateOneAsync(new CreateIndexModel<GameConfiguration>(Builders<GameConfiguration>.IndexKeys.Ascending(room => room.RoomName))),
                GetCollection<GameConfiguration>().Indexes.CreateOneAsync(new CreateIndexModel<GameConfiguration>(Builders<GameConfiguration>.IndexKeys.Ascending(room => room.UnixTimeCreated))),
                GetCollection<GameConfiguration>().Indexes.CreateOneAsync(new CreateIndexModel<GameConfiguration>(Builders<GameConfiguration>.IndexKeys.Ascending(room => room.Creator.Id))),
                GetCollection<GameConfiguration>().Indexes.CreateOneAsync(new CreateIndexModel<GameConfiguration>(Builders<GameConfiguration>.IndexKeys.Ascending(room => room.Creator.Username))),
                GetCollection<GameConfiguration>().Indexes.CreateOneAsync(new CreateIndexModel<GameConfiguration>(Builders<GameConfiguration>.IndexKeys.Ascending(room => room.GameVersion))),
                GetCollection<GameConfiguration>().Indexes.CreateOneAsync(new CreateIndexModel<GameConfiguration>(Builders<GameConfiguration>.IndexKeys.Ascending(room => room.RoomStatus))),
                
                // Index Friend relations
                GetCollection<Friend>().Indexes.CreateOneAsync(new CreateIndexModel<Friend>(Builders<Friend>.IndexKeys.Ascending(relation => relation.PlayerId))),
                GetCollection<Friend>().Indexes.CreateOneAsync(new CreateIndexModel<Friend>(Builders<Friend>.IndexKeys.Ascending(relation => relation.FriendId))),
                GetCollection<Friend>().Indexes.CreateOneAsync(new CreateIndexModel<Friend>(Builders<Friend>.IndexKeys.Ascending(relation => relation.RelationshipStatus))),

                // Index Game Events
                GetCollection<GameEventData>().Indexes.CreateOneAsync(new CreateIndexModel<GameEventData>(Builders<GameEventData>.IndexKeys.Ascending(gameEvent => gameEvent.Id))),
                GetCollection<GameEventData>().Indexes.CreateOneAsync(new CreateIndexModel<GameEventData>(Builders<GameEventData>.IndexKeys.Ascending(gameEvent => gameEvent.IssuedBy))),
                GetCollection<GameEventData>().Indexes.CreateOneAsync(new CreateIndexModel<GameEventData>(Builders<GameEventData>.IndexKeys.Ascending(gameEvent => gameEvent.UnixTimeIssued))),
                GetCollection<GameEventData>().Indexes.CreateOneAsync(new CreateIndexModel<GameEventData>(Builders<GameEventData>.IndexKeys.Ascending(gameEvent => gameEvent.OccursAtTick))),

                // Index group chats.
                GetCollection<ChatMessage>().Indexes.CreateOneAsync(new CreateIndexModel<ChatMessage>(Builders<ChatMessage>.IndexKeys.Ascending(message => message.RoomId))),
                GetCollection<ChatMessage>().Indexes.CreateOneAsync(new CreateIndexModel<ChatMessage>(Builders<ChatMessage>.IndexKeys.Ascending(message => message.GroupId))),
                GetCollection<ChatMessage>().Indexes.CreateOneAsync(new CreateIndexModel<ChatMessage>(Builders<ChatMessage>.IndexKeys.Ascending(message => message.SentBy))),
                GetCollection<ChatMessage>().Indexes.CreateOneAsync(new CreateIndexModel<ChatMessage>(Builders<ChatMessage>.IndexKeys.Ascending(message => message.UnixTimeCreatedAt))),
                GetCollection<ChatMessage>().Indexes.CreateOneAsync(new CreateIndexModel<ChatMessage>(Builders<ChatMessage>.IndexKeys.Text(message => message.Message))),
                
                // Index message groups
                GetCollection<MessageGroupDatabaseModel>().Indexes.CreateOneAsync(new CreateIndexModel<MessageGroupDatabaseModel>(Builders<MessageGroupDatabaseModel>.IndexKeys.Ascending(group => group.Id))),
                GetCollection<MessageGroupDatabaseModel>().Indexes.CreateOneAsync(new CreateIndexModel<MessageGroupDatabaseModel>(Builders<MessageGroupDatabaseModel>.IndexKeys.Ascending(group => group.RoomId))),
                
                // Index Specialist Configurations
                GetCollection<SpecialistConfiguration>().Indexes.CreateOneAsync(new CreateIndexModel<SpecialistConfiguration>(Builders<SpecialistConfiguration>.IndexKeys.Ascending(spec => spec.Id))),
                GetCollection<SpecialistConfiguration>().Indexes.CreateOneAsync(new CreateIndexModel<SpecialistConfiguration>(Builders<SpecialistConfiguration>.IndexKeys.Ascending(spec => spec.Creator.Id))),
                GetCollection<SpecialistConfiguration>().Indexes.CreateOneAsync(new CreateIndexModel<SpecialistConfiguration>(Builders<SpecialistConfiguration>.IndexKeys.Ascending(spec => spec.PromotesFromSpecialistId))),
                GetCollection<SpecialistConfiguration>().Indexes.CreateOneAsync(new CreateIndexModel<SpecialistConfiguration>(Builders<SpecialistConfiguration>.IndexKeys.Text(spec => spec.SpecialistName))),
                
                // Index Specialist Configurations
                GetCollection<SpecialistPackage>().Indexes.CreateOneAsync(new CreateIndexModel<SpecialistPackage>(Builders<SpecialistPackage>.IndexKeys.Ascending(package => package.Id))),
                GetCollection<SpecialistPackage>().Indexes.CreateOneAsync(new CreateIndexModel<SpecialistPackage>(Builders<SpecialistPackage>.IndexKeys.Ascending(package => package.Creator.Id))),
                GetCollection<SpecialistPackage>().Indexes.CreateOneAsync(new CreateIndexModel<SpecialistPackage>(Builders<SpecialistPackage>.IndexKeys.Text(package => package.PackageName))),
                
                // Index Specialist Configurations
                GetCollection<ServerActionLog>().Indexes.CreateOneAsync(new CreateIndexModel<ServerActionLog>(Builders<ServerActionLog>.IndexKeys.Ascending(action => action.Id))),
                GetCollection<ServerActionLog>().Indexes.CreateOneAsync(new CreateIndexModel<ServerActionLog>(Builders<ServerActionLog>.IndexKeys.Ascending(action => action.Username))),
                GetCollection<ServerActionLog>().Indexes.CreateOneAsync(new CreateIndexModel<ServerActionLog>(Builders<ServerActionLog>.IndexKeys.Ascending(action => action.UserId))),
                GetCollection<ServerActionLog>().Indexes.CreateOneAsync(new CreateIndexModel<ServerActionLog>(Builders<ServerActionLog>.IndexKeys.Ascending(action => action.RequestUrl))),
                GetCollection<ServerActionLog>().Indexes.CreateOneAsync(new CreateIndexModel<ServerActionLog>(Builders<ServerActionLog>.IndexKeys.Ascending(action => action.StatusCode))),
                GetCollection<ServerActionLog>().Indexes.CreateOneAsync(new CreateIndexModel<ServerActionLog>(Builders<ServerActionLog>.IndexKeys.Ascending(action => action.UnixTimeProcessed))),
                
                // Index Specialist Configurations
                GetCollection<ServerExceptionLog>().Indexes.CreateOneAsync(new CreateIndexModel<ServerExceptionLog>(Builders<ServerExceptionLog>.IndexKeys.Ascending(action => action.Id))),
                GetCollection<ServerExceptionLog>().Indexes.CreateOneAsync(new CreateIndexModel<ServerExceptionLog>(Builders<ServerExceptionLog>.IndexKeys.Ascending(action => action.Username))),
                GetCollection<ServerExceptionLog>().Indexes.CreateOneAsync(new CreateIndexModel<ServerExceptionLog>(Builders<ServerExceptionLog>.IndexKeys.Ascending(action => action.UserId))),
                GetCollection<ServerExceptionLog>().Indexes.CreateOneAsync(new CreateIndexModel<ServerExceptionLog>(Builders<ServerExceptionLog>.IndexKeys.Ascending(action => action.RequestUri))),
                GetCollection<ServerExceptionLog>().Indexes.CreateOneAsync(new CreateIndexModel<ServerExceptionLog>(Builders<ServerExceptionLog>.IndexKeys.Ascending(action => action.RemoteIpAddress))),
                GetCollection<ServerExceptionLog>().Indexes.CreateOneAsync(new CreateIndexModel<ServerExceptionLog>(Builders<ServerExceptionLog>.IndexKeys.Ascending(action => action.UnixTimeProcessed))),
            };

            
            _logger.LogInformation("Creating database indexes");
            await Task.WhenAll(indexTasks);
            _logger.LogInformation("Database created.");
        }

        public static IMongoCollection<T> GetCollection<T>()
        {
            return _database.GetCollection<T>(typeof(T).ToString());
        }

        public void FlushCollections()
        {
            _logger.LogInformation("Flushing database!");
            GetCollection<UserModel>().DeleteMany(FilterDefinition<UserModel>.Empty);
            GetCollection<UserIpAddressLink>().DeleteMany(FilterDefinition<UserIpAddressLink>.Empty);
            GetCollection<GameConfiguration>().DeleteMany(FilterDefinition<GameConfiguration>.Empty);
            GetCollection<Friend>().DeleteMany(FilterDefinition<Friend>.Empty);
            GetCollection<GameEventData>().DeleteMany(FilterDefinition<GameEventData>.Empty);
            GetCollection<ChatMessage>().DeleteMany(FilterDefinition<ChatMessage>.Empty);
            GetCollection<MessageGroupDatabaseModel>().DeleteMany(FilterDefinition<MessageGroupDatabaseModel>.Empty);
            GetCollection<SpecialistConfiguration>().DeleteMany(FilterDefinition<SpecialistConfiguration>.Empty);
            GetCollection<SpecialistPackage>().DeleteMany(FilterDefinition<SpecialistPackage>.Empty);
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