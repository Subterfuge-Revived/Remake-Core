using System;
using System.Collections.Generic;

namespace SubterfugeCore.Models.GameEvents
{
    // This data structure is ONLY for administrators.
    // Players will NOT get this information.
    public class DetailedUser
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string DeviceIdentifier { get; set; }
        public string PhoneNumberHash { get; set; }
        public string DiscordUsername { get; set; }
        public UserClaim[] Claims { get; set; }
        public List<MultiboxAccount> MultiboxAccounts { get; set; }
        public string PushNotificationIdentifier { get; set; }
        public string DeviceType { get; set; }
        
        // Administrative
        public DateTime DateCreated { get; set; }
        public DateTime BannedUntil { get; set; }
        
        public List<BanHistory> BanHistory { get; set; }
    }

    public class SimpleUser
    {
        public string Id { get; set; }
        public string Username { get; set; }
    }
    
    public class User
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public UserClaim[] Claims { get; set; }
        // A list of other accounts that this user has associated as.
        public List<SimpleUser> Pseudonyms { get; set; } = new List<SimpleUser>() { };
        
        // Administrative
        public DateTime DateCreated { get; set; }
        public DateTime BannedUntil { get; set; }
    }

    public enum DeviceType
    {
        Desktop,
        iOS,
        Android,
        Windows,
    }
    
    public class MultiboxAccount
    {
        public SimpleUser User { get; set; }
        public MultiBoxReason MultiboxReason { get; set; }
        public DateTime TimeOccured { get; set; } = DateTime.UtcNow;
    }

    public enum MultiBoxReason
    {
        NONE,
        LOGIN_WITH_CREDENTIALS_FOR_ANOTHER_ACCOUNT,
        DUPLICATE_PHONE_NUMBER,
        DUPLICATE_DEVICE_ID,
        DUPLICATE_DISCORD_USERNAME,
        DUPLICATE_EMAIL,
    }

    public class BanHistory
    {
        public string Reason { get; set; }
        public string AdministratorNotes { get; set; }
        public DateTime DateExpires { get; set; }
        public DateTime DateApplied { get; set; } = DateTime.UtcNow;
    }
}