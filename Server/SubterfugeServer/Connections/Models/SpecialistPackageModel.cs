using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf;
using StackExchange.Redis;
using SubterfugeRemakeService;

namespace SubterfugeServerConsole.Connections.Models
{
    public class SpecialistPackageModel
    {
        public SpecialistPackage SpecialistPackage;


        public SpecialistPackageModel(SpecialistPackage package)
        {
            SpecialistPackage = package;
            if (String.IsNullOrEmpty(SpecialistPackage.SpecialistPackageId))
            {
                SpecialistPackage.SpecialistPackageId = Guid.NewGuid().ToString();
            }
        }
        
        public SpecialistPackageModel(RedisValue byteArray)
        {
            SpecialistPackage = SpecialistPackage.Parser.ParseFrom(byteArray);
        }

        public async Task<bool> saveToRedis()
        {
            HashEntry[] specialistRecord =
            {
                new HashEntry(SpecialistPackage.SpecialistPackageId, SpecialistPackage.ToByteArray()),
            };

            await RedisConnector.Redis.SetAddAsync($"user:{SpecialistPackage.Creator}:specialistPackages", SpecialistPackage.SpecialistPackageId);
            await RedisConnector.Redis.HashSetAsync(specialistPackageKey(), specialistRecord);
            return true;
        }
        
        public static async Task<SpecialistPackageModel> fromId(string packageId)
        {
            RedisValue specialistData = await RedisConnector.Redis.HashGetAsync("specialists:packages", packageId);
            if (specialistData.HasValue)
            {
                return new SpecialistPackageModel(specialistData);
            }

            return null;
        }
        
        public static async Task<List<SpecialistPackageModel>> search(string searchTerm)
        {
            HashEntry[] values = await RedisConnector.Redis.HashGetAllAsync("specialists:packages");
            List<SpecialistPackageModel> results = new List<SpecialistPackageModel>();
            
            foreach(HashEntry entry in values)
            {
                SpecialistPackageModel model = new SpecialistPackageModel(entry.Value);
                if (model.SpecialistPackage.Creator.Username.Contains(searchTerm) ||
                    model.SpecialistPackage.PackageName.Contains(searchTerm))
                {
                    results.Add(model);
                }
            }

            return results;
        }
        
        private string specialistPackageKey()
        {
            return "specialists:packages";
        }
        
    }
}