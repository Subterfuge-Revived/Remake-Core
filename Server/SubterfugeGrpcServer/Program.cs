﻿using System;
using System.Threading;
using Grpc.Core;
using Grpc.Core.Interceptors;
using SubterfugeRemakeService;
using SubterfugeServerConsole.Connections;
 using SubterfugeServerConsole.Connections.Models;

 namespace SubterfugeServerConsole
{
    class Program
    {
        private const String Hostname = "server"; // For docker
        // private const String Hostname = "localhost"; // For local
        private const int Port = 5000;
        
        private const String dbHost = "db"; // For docker
        // private const String dbHost = "localhost"; // For local
        private const int dbPort = 27017;
        
            
        public static ManualResetEvent Shutdown = new ManualResetEvent(false);
        
        static void Main(string[] args)
        {
            MongoConnector mongo = new MongoConnector(dbHost, dbPort, false);
            SubterfugeServer grpcService = new SubterfugeServer();

            Server server = new Server
            {
                Services = {subterfugeService.BindService(grpcService).Intercept(new LoggerInterceptor()).Intercept(new JwtInterceptor())},
                Ports = {new ServerPort(Hostname, Port, ServerCredentials.Insecure)}
            };
            
            Console.WriteLine($"Listening on {Port}...");
            DbUserModel.CreateSuperUser(); // Creates a super user with admin powers.
            server.Start();
            Shutdown.WaitOne();
            server.ShutdownAsync().Wait();
        }
        
    }
}