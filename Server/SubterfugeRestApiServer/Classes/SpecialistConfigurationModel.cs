using MongoDB.Driver;
using SubterfugeCore.Models.GameEvents;

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

        public async Task<bool> Save()
        {
            await MongoConnector.GetCollection<SpecialistConfiguration>().InsertOneAsync(SpecialistConfig);
            return true;
        }

        public static async Task<SpecialistConfigurationModel> fromId(string specialistId)
        {
            SpecialistConfiguration config = (await MongoConnector.GetCollection<SpecialistConfiguration>()
                .FindAsync(it => it.Id == specialistId))
                .ToList()
                .FirstOrDefault();
            
            if (config != null)
            {
                return new SpecialistConfigurationModel(config);
            }

            return null;
        }
        
        public static async Task<List<SpecialistConfigurationModel>> Search(string searchTerm)
        {
            return (await MongoConnector.GetCollection<SpecialistConfiguration>()
                .FindAsync(it => it.Creator.Username.Contains(searchTerm) || it.SpecialistName.Contains(searchTerm)))
                .ToList()
                .Select(it => new SpecialistConfigurationModel(it))
                .ToList();
        }
    }
}