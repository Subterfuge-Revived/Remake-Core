using SubterfugeCore.Models.GameEvents;

namespace SubterfugeDatabaseProvider.Models;

public class DbChatMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.MaxValue;
    public string RoomId { get; set; }
    public string GroupId { get; set; }
    public User SentBy { get; set; }
    public string Message { get; set; }

    public ChatMessage ToChatMessage()
    {
        return new ChatMessage()
        {
            Id = Id,
            SentAt = SentAt,
            RoomId = RoomId,
            GroupId = GroupId,
            SentBy = SentBy,
            Message = Message,
        };
    }
}