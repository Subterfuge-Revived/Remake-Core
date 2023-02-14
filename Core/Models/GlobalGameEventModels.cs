using System;
using System.Collections.Generic;

namespace SubterfugeCore.Models.GameEvents
{
    public class CreateAnnouncementRequest
    {
        public string Title { get; set; }
        public string Message { get; set; }
        
        /**
         * Who to send the message to.
         * If 'global', everyone will see the message. Otherwise, a comma-separated list of user IDs should be provided.
         */
        public string BroadcastTo { get; set; } = "global";
        public DateTime StartsAt { get; set; } = DateTime.Now;
        public DateTime ExpiresAt { get; set; } = DateTime.Now.AddDays(21);
    }

    public class CreateAnnouncementResponse
    {
        public string AnnouncementId { get; set; }
    }

    public class GetAnnouncementsResponse
    {
        public List<GameAnnouncement> Announcements { get; set; }
    }

    public class GameAnnouncement
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public SimpleUser PostedBy { get; set; }
        public DateTime StartsAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(21);
    }
}