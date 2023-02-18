using Subterfuge.Remake.Api.Network;

namespace Subterfuge.Remake.Server.Database.Models;

public class DbIpBan
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime DateApplied { get; set; } = DateTime.UtcNow;
    public string IpAddressOrRegex { get; set; }
    public DateTime BannedUntil { get; set; }
    public string Reason { get; set; }
    public string AdminNotes { get; set; }

    public IpBans ToIpBan()
    {
        return new IpBans()
        {
            Reason = Reason,
            AdminNotes = AdminNotes,
            BannedUntil = BannedUntil,
            DateApplied = DateApplied,
            IpOrRegex = IpAddressOrRegex
        };
    }
}