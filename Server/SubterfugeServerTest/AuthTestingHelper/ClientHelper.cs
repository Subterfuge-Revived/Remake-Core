using System;
using SubterfugeServerConsole.Connections;

namespace Tests.AuthTestingHelper
{
    public class ClientHelper
    {
        
        public static SubterfugeClient.SubterfugeClient GetClient()
        {
            String Hostname = "server"; // For docker
            // String Hostname = "localhost"; // For local
            int Port = 5000;
                    
            String dbHost = "db"; // For docker
            // String dbHost = "localhost"; // For local
            int dbPort = 6379;
            
            MongoConnector mongo = new MongoConnector(dbHost, dbPort.ToString(), true);
            return new SubterfugeClient.SubterfugeClient(Hostname, Port.ToString());
        }
    }
}