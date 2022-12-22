using SubterfugeCore.Models.GameEvents;

namespace SubterfugeDatabaseProvider.Models;

public class DbServerException
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime UnixTimeProcessed { get; set; } = DateTime.UtcNow;
    public DateTime ExpireAt { get; set; } = DateTime.UtcNow.AddMonths(12);
    public string? Username { get; set; }
    public string? UserId { get; set; }
    public string? RemoteIpAddress { get; set; }
    public string? HttpMethod { get; set; }
    public string? RequestUri { get; set; }
    public string? RequestBody { get; set; }
    public string? QueryString { get; set; }
    public string? UserAgent { get; set; }
    public string? ExceptionMessage { get; set; }
    public string? ExceptionSource { get; set; }
    public string? StackTrace { get; set; }

    public ServerException ToServerException()
    {
        return new ServerException()
        {
            Id = Id,
            Username = Username,
            UserId = UserId,
            RemoteIpAddress = RemoteIpAddress,
            HttpMethod = HttpMethod,
            RequestUri = RequestUri,
            RequestBody = RequestBody,
            QueryString = QueryString,
            ExceptionMessage = ExceptionMessage,
            ExceptionSource = ExceptionSource,
            StackTrace = StackTrace,
            UnixTimeProcessed = UnixTimeProcessed,
        };
    }
}