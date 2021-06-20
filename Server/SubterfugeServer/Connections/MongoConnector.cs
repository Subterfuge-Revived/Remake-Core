using MongoDB.Driver;
using SubterfugeRemakeService;

namespace SubterfugeServerConsole.Connections
{
    public class MongoConnector
    {
        public static IMongoDatabase Database;
        
        public MongoConnector(string hostname, string port, bool allowAdmin)
        {
            MongoClientSettings settings = new MongoClientSettings();
            settings.Credential = MongoCredential.CreateCredential("subterfugeDb", "user", "pass");
            settings.Server = MongoServerAddress.Parse($"{hostname}:{port}");
            settings.ApplicationName = "SubterfugeServer";
            
            var client = new MongoClient(settings);
            Database = client.GetDatabase("subterfugeDb");
        }

        public static IMongoCollection<UserModel> getUserCollection()
        {
            return Database.GetCollection<UserModel>("Users");
        }
        
        public static IMongoCollection<RoomModel> getGameRoomCollection()
        {
            return Database.GetCollection<RoomModel>("GameRooms");
        }
        
        public static IMongoCollection<FriendModel> getFriendCollection()
        {
            return Database.GetCollection<FriendModel>("Friends");
        }

        public static IMongoCollection<GameEventModel> getGameEventCollection()
        {
            return Database.GetCollection<GameEventModel>("GameEvents");
        }

        public static IMongoCollection<UserModel> getGroupChatCollection()
        {
            return Database.GetCollection<UserModel>("GroupChats");
        }
        
    }
}