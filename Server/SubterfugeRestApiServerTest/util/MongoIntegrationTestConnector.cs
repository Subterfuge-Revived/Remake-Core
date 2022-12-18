using System.Security.Cryptography;
using MongoDB.Driver;
using SubterfugeCore.Models.GameEvents;

namespace SubterfugeServerConsole.Connections
{
    public class MongoIntegrationTestConnector
    {
        private static IMongoDatabase _database;
        
        public MongoIntegrationTestConnector()
        {
            string hostname = "localhost";
            int port = 27017;
            
            // Get environment
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environment == "Docker")
            {
                hostname = "db";
            }

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
        
        public async Task<UserModel> CreateTestingSuperUser()
        {
            var userModel = new UserModel()
            {
                Id = "1",
                Username = "admin",
                Email = "admin@admin.com",
                EmailVerified = true,
                PasswordHash = HashPassword("admin"),
                Claims = new[] { UserClaim.User, UserClaim.Administrator, UserClaim.EmailVerified }
            };
            
            await GetUserCollection().InsertOneAsync(userModel);
            return userModel;
        }
        
        private String HashPassword(string password)
        {
            // Create salt
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
            
            // Create the Rfc2898DeriveBytes and get the hash value: 
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000);
            byte[] hash = pbkdf2.GetBytes(20);
            
            // Combine salt and password bytes
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);
            
            // Return value
            return Convert.ToBase64String(hashBytes);
        }
        
    }
}