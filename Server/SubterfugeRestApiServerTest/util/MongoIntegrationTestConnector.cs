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

        public static IMongoCollection<T> GetCollection<T>()
        {
            return _database.GetCollection<T>(typeof(T).ToString());
        }

        public void FlushCollections()
        {
            GetCollection<UserModel>().DeleteMany(FilterDefinition<UserModel>.Empty);
            GetCollection<UserIpAddressLink>().DeleteMany(FilterDefinition<UserIpAddressLink>.Empty);
            GetCollection<GameConfiguration>().DeleteMany(FilterDefinition<GameConfiguration>.Empty);
            GetCollection<Friend>().DeleteMany(FilterDefinition<Friend>.Empty);
            GetCollection<GameEventData>().DeleteMany(FilterDefinition<GameEventData>.Empty);
            GetCollection<ChatMessage>().DeleteMany(FilterDefinition<ChatMessage>.Empty);
            GetCollection<MessageGroupDatabaseModel>().DeleteMany(FilterDefinition<MessageGroupDatabaseModel>.Empty);
            GetCollection<SpecialistConfiguration>().DeleteMany(FilterDefinition<SpecialistConfiguration>.Empty);
            GetCollection<SpecialistPackage>().DeleteMany(FilterDefinition<SpecialistPackage>.Empty);
            // Don't clear these collections.
            // They are very helpful for debugging and tracking server activity.
            
            // GetServerActionLogCollection().DeleteMany(FilterDefinition<ServerActionLog>.Empty);
            // GetServerExceptionLogCollection().DeleteMany(FilterDefinition<ServerExceptionLog>.Empty);
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
            
            await GetCollection<UserModel>().InsertOneAsync(userModel);
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