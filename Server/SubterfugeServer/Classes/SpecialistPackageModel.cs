using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using SubterfugeRemakeService;

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
            await MongoConnector.GetSpecialistPackageCollection().InsertOneAsync(new SpecialistPackageMapper(SpecialistPackage));
            return true;
        }
        
        public static async Task<SpecialistPackageModel> fromId(string packageId)
        {
            SpecialistPackageMapper mapper = MongoConnector.GetSpecialistPackageCollection().Find(it => it.Id == packageId).FirstOrDefault();
            if (mapper != null)
            {
                return new SpecialistPackageModel(mapper.ToProto());
            }

            return null;
        }
        
        public static async Task<List<SpecialistPackageModel>> search(string searchTerm)
        {
            return MongoConnector.GetSpecialistPackageCollection()
                .Find(it => it.Creator.Username.Contains(searchTerm) || 
                            it.PackageName.Contains(searchTerm))
                .ToList()
                .Select(it => new SpecialistPackageModel(it.ToProto()))
                .ToList();
        }
        
    }
}