using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf;
using StackExchange.Redis;
using SubterfugeRemakeService;

namespace SubterfugeServerConsole.Connections.Models
{
    public class SpecialistConfigurationModel
    {
        public SpecialistConfiguration SpecialistConfig;
        
        public SpecialistConfigurationModel(SpecialistConfiguration configuration)
        {
            SpecialistConfig = configuration;
            // Generate an id if one was not generated
            if (string.IsNullOrEmpty(SpecialistConfig.SpecialistId))
            {
                SpecialistConfig.SpecialistId = Guid.NewGuid().ToString();
            }

        }
        
        public SpecialistConfigurationModel(RedisValue redisValue)
        {
            SpecialistConfig = SpecialistConfiguration.Parser.ParseFrom(redisValue);
        }

        public SpecialistConfigurationModel(int specialistId)
        {
            // Load spec with ID from redis.
        }

        public async Task<bool> saveToRedis()
        {
            HashEntry[] specialistRecord =
            {
                new HashEntry(SpecialistConfig.SpecialistId, SpecialistConfig.ToByteArray()),
            };

            await RedisConnector.Redis.HashSetAsync(specialistConfigurationKey(), specialistRecord);
            await RedisConnector.Redis.SetAddAsync($"user:{SpecialistConfig.Creator}:specialists", SpecialistConfig.SpecialistId);
            return true;
        }

        public static async Task<SpecialistConfigurationModel> fromId(string specialistId)
        {
            RedisValue specialistData = await RedisConnector.Redis.HashGetAsync("specialists", specialistId);
            if (specialistData.HasValue)
            {
                return new SpecialistConfigurationModel(specialistData);
            }

            return null;
        }
        
        public static async Task<List<SpecialistConfigurationModel>> search(string searchTerm)
        {
            HashEntry[] values = await RedisConnector.Redis.HashGetAllAsync("specialists");
            List<SpecialistConfigurationModel> results = new List<SpecialistConfigurationModel>();
            
            foreach(HashEntry entry in values)
            {
                SpecialistConfigurationModel model = new SpecialistConfigurationModel(entry.Value);
                if (model.SpecialistConfig.Creator.Username.Contains(searchTerm) ||
                    model.SpecialistConfig.SpecialistName.Contains(searchTerm))
                {
                    results.Add(model);
                }
            }

            return results;
        }

        private string specialistConfigurationKey()
        {
            return "specialists";
        }
    }
}