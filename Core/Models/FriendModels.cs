namespace SubterfugeCore.Models.GameEvents
{
    public class Friend
    {
        public string Id { get; set; }
        public string PlayerId { get; set; }
        public string FriendId { get; set; }
        public RelationshipStatus RelationshipStatus { get; set; }
        public long UnixTimeCreated { get; set; }

        public string GetFriendId(string friendToUser)
        {
            return PlayerId == friendToUser ? FriendId : PlayerId;
        }
    }

    public enum RelationshipStatus
    {
        NoRelation,
        Pending,
        Friends,
        Blocked,
    }
    
    
}