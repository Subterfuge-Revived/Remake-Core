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
        public List<UserClaim> Claims { get; set; }
        public string PushNotificationIdentifier { get; set; }
        public string DeviceType { get; set; }

        public User ToUser()
        {
            return new User()
            {
                Id = Id,
                Username = Username
            };
        }
    }

    public enum DeviceType
    {
        Desktop,
        iOS,
        Android,
        Windows,
    }
}