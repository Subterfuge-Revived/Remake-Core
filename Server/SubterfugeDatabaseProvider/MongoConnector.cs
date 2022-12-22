using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SubterfugeCore.Models.GameEvents;
using SubterfugeDatabaseProvider.Models;
using SubterfugeRestApiServer.Authentication;
using SubterfugeServerConsole.Connections.Collections;

namespace SubterfugeServerConsole.Connections
{
    public class MongoConnector : IDatabaseCollectionProvider
    {
        private static IMongoDatabase _database;
        private ILogger? _logger;
        private MongoConfiguration _config;

        public MongoConnector(IMongoConfigurationProvider config, ILogger<MongoConnector>? logger)
        {
            _logger = logger;
            _config = config.GetConfiguration();
            
            _logger?.LogInformation("Configuring MongoDB...");
            _database = ConnectToMongo();
            
            AddComponent(new UserModelCollection(GetDbCollection<DbUserModel>()));
            AddComponent(new GameConfigurationCollection(GetDbCollection<DbGameLobbyConfiguration>()));
            AddComponent(new RelationshipCollection(GetDbCollection<DbPlayerRelationship>()));
            AddComponent(new GameEventCollection(GetDbCollection<DbGameEvent>()));
            AddComponent(new ChatMessageCollection(GetDbCollection<DbChatMessage>()));
            AddComponent(new GroupCollection(GetDbCollection<DbMessageGroup>()));
            AddComponent(new SpecialistConfigurationCollection(GetDbCollection<DbSpecialistConfiguration>()));
            AddComponent(new SpecialistPackageCollection(GetDbCollection<DbSpecialistPackage>()));
            AddComponent(new ServerActionLogCollection(GetDbCollection<DbServerAction>()));
            AddComponent(new ServerExceptionLogCollection(GetDbCollection<DbServerException>()));

            logger?.LogInformation("Connected to database. Creating indexes...");
            
            CreateAllIndexes();
            
            if (_config.FlushDatabase)
                FlushAll();

            if (_config.CreateSuperUser)
                CreateSuperUser();
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

        private IMongoCollection<T> GetDbCollection<T>()
        {
            return _database.GetCollection<T>(typeof(T).ToString());
        }

        public async Task<DbUserModel> CreateSuperUser()
        {
            DbUserModel dbUserModel = new DbUserModel()
            {
                Id = "1",
                Username = _config.SuperUserUsername,
                Email = _config.SuperUserUsername,
                EmailVerified = true,
                PasswordHash = JwtManager.HashString(_config.SuperUserPassword),
                Claims = new [] { UserClaim.User, UserClaim.Administrator, UserClaim.EmailVerified }
            };
            await GetDbCollection<DbUserModel>().ReplaceOneAsync(
                it => it.Id == dbUserModel.Id, dbUserModel, new UpdateOptions(){ IsUpsert = true});
            _logger?.LogInformation("SuperUser Username: {username}", _config.SuperUserUsername);
            _logger?.LogInformation("SuperUser Password: {password}", _config.SuperUserPassword);
            return dbUserModel;
        }
    }
}