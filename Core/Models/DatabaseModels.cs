using System;
using System.Collections.Generic;

namespace SubterfugeCore.Models.GameEvents
{
    public class UserModel
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public string DeviceIdentifier { get; set; }
        public string PhoneNumber { get; set; }
        public Boolean EmailVerified { get; set; }
        public UserClaim[] Claims { get; set; }
        public string PushNotificationIdentifier { get; set; }
        public string DeviceType { get; set; }
        
        // Administrative
        public DateTime DateCreated { get; set; }
        public DateTime BannedUntil { get; set; }

        // Strips administrative information out.
        // Always call this before returning from the API.
        public UserModel Obfuscate()
        {
            return new UserModel()
            {
                Id = this.Id,
                Username = this.Username,
                Email = this.Email,
                EmailVerified = this.EmailVerified,
                Claims = this.Claims,
                DateCreated = this.DateCreated,
                BannedUntil = this.BannedUntil,
            };
        }

        public User ToUser()
        {
            return new User()
            {
                Id = Id,
                Username = Username
            };
        }
    }

    public class UserIpAddressLink
    {
        public string UserId { get; set; }    
        public string IpAddress { get; set; }
        public int TimesAccessed { get; set; }
    }

    public enum DeviceType
    {
        Desktop,
        iOS,
        Android,
        Windows,
    }
}