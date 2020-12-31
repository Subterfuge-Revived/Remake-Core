using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using StackExchange.Redis;
using SubterfugeServerConsole.Connections.Models;

namespace SubterfugeServerConsole.Connections
{
    public class RedisConnector
    {
        public static IDatabase redis;

        public RedisConnector(string hostname, string port)
        {
            var options = ConfigurationOptions.Parse($"{hostname}:{port}");
            options.Password = "TODO:changethis";
            ConnectionMultiplexer muxer = ConnectionMultiplexer.Connect(options);
            redis = muxer.GetDatabase();
        }
    }
}