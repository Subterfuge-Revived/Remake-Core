using MongoDB.Bson;
using MongoDB.Driver;
using SubterfugeRemakeService;

namespace SubterfugeServerConsole.Connections
{
    public class MongoDbFilters
    {
        public class RoomFilters
        {
            public static FilterDefinition<BsonDocument> WhereStatus(RoomStatus status)
            {
                return Builders<BsonDocument>.Filter.Eq("RoomStatus", status.ToBson());
            }
        }
    }
}