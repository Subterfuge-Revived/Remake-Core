namespace SubterfugeServerConsole.Connections;

public class MongoConfiguration
{
    public string Host { get; set; }
    public int Port { get; set; }
    public Boolean CreateSuperUser { get; set; }
    public string SuperUserUsername { get; set; }
    public string SuperUserPassword { get; set; }
    public Boolean FlushDatabase { get; set; }

    public MongoConfiguration(IConfiguration config)
    {
        Host = config["Host"];
        Port = Convert.ToInt32(config["Port"]);
        CreateSuperUser = Convert.ToBoolean(config["SuperUser:CreateSuperUser"]);
        SuperUserUsername = config["SuperUser:Username"];
        SuperUserPassword = config["SuperUser:Password"];
        FlushDatabase = Convert.ToBoolean(config["FlushDatabase"]);
    }
}