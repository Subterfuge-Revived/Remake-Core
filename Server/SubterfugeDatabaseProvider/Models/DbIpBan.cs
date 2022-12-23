using SubterfugeCore.Models.GameEvents;

namespace SubterfugeDatabaseProvider.Models;

public class DbIpBan
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime DateApplied { get; set; } = DateTime.UtcNow;
    public string IpAddressOrRegex { get; set; }
    public DateTime BannedUntil { get; set; }
    public string AdminNotes { get; set; }

    public IpBans ToIpBan()
    {
        return new IpBans()
        {
            AdminNotes = AdminNotes,
            BannedUntil = BannedUntil,
            DateApplied = DateApplied,
            IpOrRegex = IpAddressOrRegex
        };
    }
}