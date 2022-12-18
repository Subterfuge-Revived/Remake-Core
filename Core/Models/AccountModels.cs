using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SubterfugeCore.Models.GameEvents
{

    public class User
    {
        public string Id { get; set; }
        public string Username { get; set; }
    }
    
    public class AuthorizationRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class AuthorizationResponse : NetworkResponse
    {
        public User User { get; set; }
        public string Token { get; set; }
    }

    public class AccountRegistrationRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string DeviceIdentifier { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class AccountRegistrationResponse : NetworkResponse
    {
        public User User { get; set; }
        public string Token { get; set; }
    }

    public class GetRolesResponse : NetworkResponse
    {
        public UserClaim[] Claims { get; set; }
    }

    public class UpdateRolesRequest
    {
        public UserClaim[] Claims { get; set; }
    }

    public class GetUserRequest
    {
        public string UsernameSearch { get; set; }
        public string EmailSearch { get; set; }
        public string DeviceIdentifierSearch { get; set; }
        public string UserIdSearch { get; set; }
        public UserClaim[] RequireUserClaims { get; set; }
        public Boolean isBanned { get; set; }
        public int pagination { get; set; }
    }

    public class GetUserResponse
    {
        public UserModel[] Users { get; set; }
    }

    public enum UserClaim
    {
        Unknown,
        User,
        Moderator,
        Administrator,
        EmailVerified,
        Banned,
    }
}