#nullable enable
using System.Collections.Generic;

namespace Subterfuge.Remake.Api.Network
{

    public class AuthorizationRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class AuthorizationResponse
    {
        public User User { get; set; }
        public string Token { get; set; }
    }

    public class AccountRegistrationRequest
    {
        public string Username { get; set; } = "username";
        public string Password { get; set; } = "password";
        public string DeviceIdentifier { get; set; } = "someDeviceIdentifier";
        public string PhoneNumber { get; set; } = "+13069999999";
    }

    public class AccountRegistrationResponse
    {
        public User User { get; set; } = new User();
        public string Token { get; set; } = "LoginToken";
        public bool RequirePhoneValidation = true;
    }
    
    

    public class AccountValidationRequest
    {
        public string VerificationCode { get; set; }
    }

    public class AccountVadliationResponse
    {
        public bool wasValidationSuccessful { get; set; } = false;
        public User User { get; set; } = new User();
    }

    public class GetRolesResponse
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
        Unknown = 0,
        User = 1,
        Moderator = 2,
        Administrator = 3,
        PhoneVerified = 4,
        DiscordVerified = 5,
    }
}