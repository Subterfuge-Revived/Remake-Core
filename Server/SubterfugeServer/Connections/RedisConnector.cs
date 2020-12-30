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
            ConnectionMultiplexer muxer = ConnectionMultiplexer.Connect($"{hostname}:{port}");
            redis = muxer.GetDatabase();
        }
    }
}