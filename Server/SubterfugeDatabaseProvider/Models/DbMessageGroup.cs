using SubterfugeCore.Models.GameEvents;

namespace SubterfugeDatabaseProvider.Models;

public class DbMessageGroup
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.MaxValue;
    public string RoomId { get; set; }
    public List<User> MembersInGroup { get; set; }

    public static DbMessageGroup CreateGroup(string roomId, List<User> membersInGroup)
    {
        return new DbMessageGroup()
        {
            RoomId = roomId,
            MembersInGroup = membersInGroup
        };
    }

    public MessageGroup ToMessageGroup()
    {
        return new MessageGroup()
        {
            Id = Id,
            RoomId = RoomId,
            GroupMembers = MembersInGroup,
        };
    }
}