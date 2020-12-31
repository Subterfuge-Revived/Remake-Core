using System;
using System.Threading;
using Grpc.Core;
using Grpc.Core.Interceptors;
using SubterfugeRemakeService;
using SubterfugeServerConsole.Connections;

namespace SubterfugeServerConsole
{
    class Program
    {
        private const String Hostname = "server"; // For docker
        // private const String Hostname = "localhost"; // For local
        private const int Port = 5000;
        
        private const String dbHost = "db"; // For docker
        // private const String dbHost = "localhost"; // For local
        private const int dbPort = 6379;
        
            
        public static ManualResetEvent Shutdown = new ManualResetEvent(false);
        
        static void Main(string[] args)
        {
            
            RedisConnector redis = new RedisConnector(dbHost, dbPort.ToString());
            SubterfugeServer grpcService = new SubterfugeServer(redis);

            Server server = new Server
            {
                Services = {subterfugeService.BindService(grpcService).Intercept(new JwtInterceptor())},
                Ports = {new ServerPort(Hostname, Port, ServerCredentials.Insecure)}
            };
            
            Console.WriteLine($"Listening on {Port}...");
            server.Start();
            Shutdown.WaitOne();
            server.ShutdownAsync().Wait();
        }
    }
}