using Subterfuge.Remake.Api.Network;

namespace Subterfuge.Remake.Server.Database.Models;

public class DbServerAction
{
    public string Id { get; set;  } = Guid.NewGuid().ToString();
    public DateTime TimesAccessed { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMonths(2);
    public string? Username { get; set; }
    public string? UserId { get; set; }
    public string? RemoteIpAddress { get; set; }
    public string? HttpMethod { get; set; }
    public string? RequestUrl { get; set; }
    public string? UserAgent { get; set; }
    public int? StatusCode { get; set; }

    public ServerAction ToServerAction()
    {
        return new ServerAction()
        {
            Username = Username,
            UserId = UserId,
            RemoteIpAddress = RemoteIpAddress,
            HttpMethod = HttpMethod,
            RequestUrl = RequestUrl,
            StatusCode = StatusCode,
            TimeProcessed = TimesAccessed,
        };
    }
}