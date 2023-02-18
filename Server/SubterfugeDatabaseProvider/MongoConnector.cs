using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Server.Database.Collections;
using Subterfuge.Remake.Server.Database.Models;

namespace Subterfuge.Remake.Server.Database
{
    public class MongoConnector : IDatabaseCollectionProvider
    {
        private static IMongoDatabase _database;
        private ILogger? _logger;
        private MongoConfiguration _config;
        private MongoClient client;
        
        private static string DATABASE_NAME = "subterfugeDb";

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
            AddComponent(new IpBanCollection(GetDbCollection<DbIpBan>()));
            AddComponent(new GameAnnouncementCollection(GetDbCollection<DbGameAnnouncement>()));

            _logger?.LogInformation("Connected to database.");
            try
            {
                SetupDatabase();
            }
            catch (Exception e)
            {
                _logger.LogError($"Error initializing database connection. {e.Message}: {e.StackTrace}");
                throw;
            }
        }

        public async Task SetupDatabase()
        {
            if (_config.FlushDatabase)
            {
                _logger?.LogInformation("Flushing database");
                await client.DropDatabaseAsync(DATABASE_NAME);
                FlushAll();
            }
            
            _logger?.LogInformation("Creating indexes");
            await CreateAllIndexes(_logger);

            if (_config.CreateSuperUser)
            {
                _logger?.LogInformation("Creating Super user");
                await CreateSuperUser();
            }
            
            _logger?.LogInformation("Database configuration complete");
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
            
            client = new MongoClient(settings);
            return client.GetDatabase(DATABASE_NAME);
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
                PasswordHash = JwtManager.HashString(_config.SuperUserPassword),
                PhoneNumber = "9999999999",
                Claims = new [] { UserClaim.User, UserClaim.Administrator, UserClaim.PhoneVerified }
            };
            await GetDbCollection<DbUserModel>().ReplaceOneAsync(
                it => it.Id == dbUserModel.Id, dbUserModel, new UpdateOptions(){ IsUpsert = true});
            _logger?.LogInformation("SuperUser Username: {username}", _config.SuperUserUsername);
            _logger?.LogInformation("SuperUser Password: {password}", _config.SuperUserPassword);
            return dbUserModel;
        }
    }
}