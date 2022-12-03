using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using SubterfugeRemakeService;

namespace SubterfugeServerConsole.Connections
{
    public class UserModelMapper : ProtoClassMapper<UserModel>
    {
        public string Id;
        public string Username;
        public string PasswordHash;
        public string Email;
        public string DeviceIdentifier;
        public bool EmailVerified;
        public List<UserClaim> Claims;
        public string PushNotificationIdentifier;

        /// <summary>
        /// Maps the Protobuf model to the internal class
        /// </summary>
        /// <param name="userModel"></param>
        public UserModelMapper(UserModel userModel)
        {
            Id = userModel.Id;
            Username = userModel.Username;
            PasswordHash = userModel.PasswordHash;
            Email = userModel.Email;
            DeviceIdentifier = userModel.DeviceIdentifier;
            EmailVerified = userModel.EmailVerified;
            Claims = userModel.Claims.ToList();
            PushNotificationIdentifier = userModel.PushNotificationIdentifier;
        }

        public override UserModel ToProto()
        {
            return new UserModel()
            {
                Id = Id,
                Username = Username,
                PasswordHash = PasswordHash,
                Email = Email,
                DeviceIdentifier = DeviceIdentifier,
                EmailVerified = EmailVerified,
                Claims = { Claims },
                PushNotificationIdentifier = PushNotificationIdentifier,
            };
        }
    }
}