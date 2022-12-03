using System;
using System.Collections.Generic;
using System.Linq;
using SubterfugeRemakeService;

namespace SubterfugeServerConsole.Connections
{
    public class SpecialistPackageMapper : ProtoClassMapper<SpecialistPackage>
    {
        public string Id;
        public User Creator;
        public string PackageName;
        public List<String> SpecialistIds;
        public List<String> SpecialistPackageIds;

        public SpecialistPackageMapper(SpecialistPackage package)
        {
            Id = package.Id;
            Creator = package.Creator;
            PackageName = package.PackageName;
            SpecialistIds = package.SpecialistIds.ToList();
            SpecialistPackageIds = package.SpecialistPackageIds.ToList();
        }
        
        public override SpecialistPackage ToProto()
        {
            return new SpecialistPackage()
            {
                Id = Id,
                PackageName = PackageName,
                Creator = Creator,
                SpecialistIds = { SpecialistIds },
                SpecialistPackageIds = { SpecialistPackageIds },
            };
        }
    }
}