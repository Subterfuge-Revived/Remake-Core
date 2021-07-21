using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using MongoDB.Driver;
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
            if (string.IsNullOrEmpty(SpecialistConfig.Id))
            {
                SpecialistConfig.Id = Guid.NewGuid().ToString();
            }
        }

        public async Task<bool> saveToRedis()
        {
            await MongoConnector.GetSpecialistCollection().InsertOneAsync(new SpecialistConfigurationMapper(SpecialistConfig));
            return true;
        }

        public static async Task<SpecialistConfigurationModel> fromId(string specialistId)
        {
            SpecialistConfigurationMapper mapper = MongoConnector.GetSpecialistCollection().Find(it => it.Id == specialistId).FirstOrDefault();
            if (mapper != null)
            {
                return new SpecialistConfigurationModel(mapper.ToProto());
            }

            return null;
        }
        
        public static async Task<List<SpecialistConfigurationModel>> search(string searchTerm)
        {
            return MongoConnector.GetSpecialistCollection()
                .Find(it => it.Creator.Username.Contains(searchTerm) || 
                            it.SpecialistName.Contains(searchTerm))
                .ToList()
                .Select(it => new SpecialistConfigurationModel(it.ToProto()))
                .ToList();
        }
    }
}