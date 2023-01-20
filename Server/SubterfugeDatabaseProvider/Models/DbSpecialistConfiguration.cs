using SubterfugeCore.Models.GameEvents;

namespace SubterfugeDatabaseProvider.Models;

public class DbSpecialistConfiguration
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.MaxValue;
    public long Priority { get; set; }
    public string SpecialistName { get; set; }
    public User Creator { get; set; }
    public List<SpecialistEffectConfiguration> SpecialistEffects { get; set; }
    public string? PromotesFromSpecialistId { get; set; }
    public uint TimesUsedInGame { get; set; } = 0;
    public Ratings Ratings { get; set; } = new Ratings();

    public SpecialistConfiguration ToSpecialistConfiguration()
    {
        return new SpecialistConfiguration()
        {
            Id = Id,
            Creator = Creator,
            Priority = Priority,
            PromotesFromSpecialistId = PromotesFromSpecialistId,
            SpecialistEffects = SpecialistEffects,
            SpecialistName = SpecialistName,
        };
    }

    public static DbSpecialistConfiguration FromRequest(SubmitCustomSpecialistRequest request, User creator)
    {
        return new DbSpecialistConfiguration()
        {
            Priority = request.Priority,
            SpecialistEffects = request.SpecialistEffects,
            PromotesFromSpecialistId = request.PromotesFromSpecialistId,
            Creator = creator,
            SpecialistName = request.SpecialistName,
        };
    }
}

public class Ratings
{
    public int fiveStar { get; set; } = 0;
    public int fourStar { get; set; } = 0;
    public int threeStar { get; set; } = 0;
    public int twoStar { get; set; } = 0;
    public int oneStar { get; set; } = 0;

    public double AverageRating()
    {
        if (TotalRatings() == 0)
            return 0.0;
        return ((fiveStar * 5.0) +
                (fourStar * 4) +
                (threeStar * 3) +
                (twoStar * 2) +
                 oneStar
                ) / TotalRatings();
    }

    public int TotalRatings()
    {
        return fiveStar + fourStar + threeStar + twoStar + oneStar;
    }
}