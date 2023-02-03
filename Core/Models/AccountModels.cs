#nullable enable
using System.Collections.Generic;

namespace SubterfugeCore.Models.GameEvents
{

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
        public string Username { get; set; } = "username";
        public string Password { get; set; } = "password";
        public string Email { get; set; } = "yourEmail@email.com";
        public string DeviceIdentifier { get; set; } = "someDeviceIdentifier";
        public string PhoneNumber { get; set; } = "SomePhoneNumber";
    }

    public class AccountRegistrationResponse : NetworkResponse
    {
        public User User { get; set; } = new User();
        public string Token { get; set; } = "LoginToken";
    }

    public class GetRolesResponse : NetworkResponse
    {
        public UserClaim[] Claims { get; set; } = new UserClaim[] {};
    }

    public class UpdateRolesRequest
    {
        public UserClaim[] Claims { get; set; } = new UserClaim[] {};
    }

    public class GetUserRequest
    {
        public string? UsernameSearch { get; set; } = "";
        public string? EmailSearch { get; set; } = "";
        public string? DeviceIdentifierSearch { get; set; } = "";
        public string? UserIdSearch { get; set; } = "";
        public UserClaim? RequireUserClaim { get; set; } = null;
        public bool? isBanned { get; set; } = null;
        public int pagination { get; set; } = 1;
    }

    public class GetUserResponse
    {
        public User User { get; set; }
    }

    public class GetDetailedUsersResponse
    {
        public List<DetailedUser> users { get; set; }
    }

    public enum UserClaim
    {
        Unknown,
        User,
        Moderator,
        Administrator,
        EmailVerified,
        PhoneVerified,
        DiscordVerified,
    }
}