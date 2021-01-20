using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using StackExchange.Redis;
using SubterfugeServerConsole.Connections.Models;

namespace SubterfugeServerConsole.Connections
{
    public class RedisConnector
    {
        public static IDatabase Redis;
        public static IServer Server;

        public RedisConnector(string hostname, string port, bool allowAdmin)
        {
            var options = ConfigurationOptions.Parse($"{hostname}:{port}");
            options.Password = "TODO:changethis";
            options.AllowAdmin = allowAdmin;
            ConnectionMultiplexer muxer = ConnectionMultiplexer.Connect(options);
            Redis = muxer.GetDatabase();
            Server = muxer.GetServer($"{hostname}:{port}");
        }
    }
}