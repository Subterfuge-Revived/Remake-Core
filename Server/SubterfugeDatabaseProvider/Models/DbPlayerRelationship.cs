using SubterfugeCore.Models.GameEvents;

namespace SubterfugeDatabaseProvider.Models;

public class DbPlayerRelationship
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime UnixTimeLastUpdated { get; set; } = DateTime.UtcNow;
    public User Player { get; set; }
    public User Friend { get; set; }
    public RelationshipStatus RelationshipStatus { get; set; }

    public User GetOtherUser(string userId)
    {
        return Player.Id == userId ? Friend : Player;
    }
}