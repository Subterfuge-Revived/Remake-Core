namespace Subterfuge.Remake.Api.Network
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
        NoRelation = 0,
        Pending = 1,
        Friends = 2,
        Blocked = 3,
    }
    
    
}