using SubterfugeCore.Models.GameEvents;

namespace SubterfugeDatabaseProvider.Models;

public class DbSpecialistPackage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public SimpleUser Creator { get; set; }
    public string PackageName { get; set; }
    public List<string> SpecialistIds { get; set; }
    public List<string> PackageIds { get; set; }
    public uint TimesUsedInGame { get; set; } = 0;
    public Ratings Ratings { get; set; } = new Ratings();

    public static DbSpecialistPackage FromRequest(CreateSpecialistPackageRequest request, SimpleUser creator)
    {
        return new DbSpecialistPackage()
        {
            Creator = creator,
            PackageIds = request.PackageIds,
            SpecialistIds = request.SpecialistIds,
            PackageName = request.PackageName,
        };
    }

    public SpecialistPackage ToSpecialistPackage()
    {
        return new SpecialistPackage()
        {
            Id = Id,
            Creator = Creator,
            PackageName = PackageName,
            SpecialistIds = SpecialistIds,
            PackageIds = PackageIds,
            TimesUsedInGame = TimesUsedInGame,
            AverageRating = Ratings.AverageRating(),
        };
    }
}