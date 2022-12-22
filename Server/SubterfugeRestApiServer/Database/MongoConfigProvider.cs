using SubterfugeServerConsole.Connections;

namespace SubterfugeRestApiServer.Database;

public class MongoConfigProvider : IMongoConfigurationProvider
{

    private MongoConfiguration _config;
    
    public MongoConfigProvider(IConfiguration config)
    {
        var mongoConfig = config.GetSection("MongoDb");
        _config = new MongoConfiguration()
        {
            Host = mongoConfig["Host"],
            Port = Convert.ToInt32(mongoConfig["Port"]),
            CreateSuperUser = Convert.ToBoolean(mongoConfig["SuperUser:CreateSuperUser"]),
            SuperUserUsername = mongoConfig["SuperUser:Username"],
            SuperUserPassword = mongoConfig["SuperUser:Password"],
            FlushDatabase = Convert.ToBoolean(mongoConfig["FlushDatabase"]),
        };
    }


    public MongoConfiguration GetConfiguration()
    {
        return _config;
    }
}