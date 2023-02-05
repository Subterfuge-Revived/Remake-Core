using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SubterfugeCore.Models.GameEvents;
using SubterfugeServerConsole.Connections;

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

    public async Task<MessageGroup> ToMessageGroup(IDatabaseCollection<DbChatMessage> _dbChatMessages)
    {
        return new MessageGroup()
        {
            Id = Id,
            RoomId = RoomId,
            GroupMembers = MembersInGroup,
            Messages = (await _dbChatMessages.Query()
                .Where(message => message.GroupId == Id)
                .Where(message => message.RoomId == RoomId)
                .OrderByDescending(message => message.SentAt)
                .Take(50)
                .ToListAsync())
                .Select(it => it.ToChatMessage())
                .ToList()
        };
    }
}