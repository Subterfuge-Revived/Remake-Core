using System;
using SubterfugeServerConsole.Connections;

namespace Tests.AuthTestingHelper
{
    public class ClientHelper
    {
        
        public static SubterfugeClient.SubterfugeClient GetClient()
        {
            // private const String Hostname = "server"; // For docker
            String Hostname = "localhost"; // For local
            int Port = 5000;
                    
            // private const String dbHost = "db"; // For docker
            String dbHost = "localhost"; // For local
            int dbPort = 6379;
        
            RedisConnector db = new RedisConnector(dbHost, dbPort.ToString(), true);
            return new SubterfugeClient.SubterfugeClient(Hostname, Port.ToString());
        }
    }
}