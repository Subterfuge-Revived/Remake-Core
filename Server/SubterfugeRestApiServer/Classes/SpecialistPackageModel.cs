using MongoDB.Driver;
using SubterfugeCore.Models.GameEvents;

namespace SubterfugeServerConsole.Connections.Models
{
    public class SpecialistPackageModel
    {
        public SpecialistPackage SpecialistPackage;


        public SpecialistPackageModel(SpecialistPackage package)
        {
            SpecialistPackage = package;
            if (String.IsNullOrEmpty(SpecialistPackage.Id))
            {
                SpecialistPackage.Id = Guid.NewGuid().ToString();
            }
        }

        public async Task<bool> SaveToDatabase()
        {
            await MongoConnector.GetSpecialistPackageCollection().InsertOneAsync(SpecialistPackage);
            return true;
        }
        
        public static async Task<SpecialistPackageModel> FromId(string packageId)
        {
            SpecialistPackage specialistPackage = (await MongoConnector.GetSpecialistPackageCollection()
                .FindAsync(it => it.Id == packageId))
                .ToList()
                .FirstOrDefault();
            
            if (specialistPackage != null)
            {
                return new SpecialistPackageModel(specialistPackage);
            }

            return null;
        }
        
        public static async Task<List<SpecialistPackageModel>> Search(string searchTerm)
        {
            return (await MongoConnector.GetSpecialistPackageCollection()
                .FindAsync(it => it.Creator.Username.Contains(searchTerm) || it.PackageName.Contains(searchTerm)))
                .ToList()
                .Select(it => new SpecialistPackageModel(it))
                .ToList();
        }
        
    }
}