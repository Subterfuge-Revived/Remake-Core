using System;
using System.Threading;
using Grpc.Core;
using SubterfugeRemakeService;

namespace SubterfugeServerConsole
{
    class Program
    {
        private const String Hostname = "localhost";
        private const int Port = 5000;
            
        public static ManualResetEvent Shutdown = new ManualResetEvent(false);
        
        static void Main(string[] args)
        {
            Server server = new Server
            {
                Services = {subterfugeService.BindService(new SubterfugeServer())},
                Ports = {new ServerPort(Hostname, Port, ServerCredentials.Insecure)}
            };
            Console.WriteLine($"Listening on {Port}...");
            server.Start();
            Shutdown.WaitOne();
            server.ShutdownAsync().Wait();
        }
    }
}