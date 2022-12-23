using SubterfugeCore.Models.GameEvents;
using SubterfugeRestApiServer.Authentication;

namespace SubterfugeDatabaseProvider.Models;

public class DbUserModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    // Administrative
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime BannedUntil { get; set; }
    
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Email { get; set; }
    public string EmailVerificationCode { get; set; }
    public bool EmailVerified { get; set; } = false;
    public string DeviceIdentifier { get; set; }
    public string PhoneNumber { get; set; }
    public string PhoneVerificationCode { get; set; }
    public bool PhoneVerified { get; set; } = false;
    public string DiscordUsername { get; set; }
    public string DiscordVerificationCode { get; set; }
    public bool DiscordVerified { get; set; } = false;
    public UserClaim[] Claims { get; set; } = new[] { UserClaim.User };
    public string PushNotificationIdentifier { get; set; }
    public string DeviceType { get; set; }
    // A list of other accounts that this user has associated as.
    public List<MultiboxAccount> MultiboxAccounts { get; set; } = new List<MultiboxAccount>() { };
    
    public List<AccountBan> BanHistory { get; set; } = new List<AccountBan>();

    public User ToUser()
    {
        return new User()
        {
            Id = Id,
            Username = Username,
            Claims = Claims,
            DateCreated = DateCreated,
            BannedUntil = BannedUntil,
            Pseudonyms = MultiboxAccounts.Select(it => it.User).ToList(),
        };
    }

    /**
     * This method should only be used to return user data to Administrator level accounts.
     */
    public DetailedUser ToDetailedUser()
    {
        return new DetailedUser()
        {
            Id = Id,
            Username = Username,
            Claims = Claims,
            DateCreated = DateCreated,
            BannedUntil = BannedUntil,
            Email = Email,
            EmailVerified = EmailVerified,
            DeviceIdentifier = DeviceIdentifier,
            PhoneNumberHash = JwtManager.HashString(PhoneNumber),
            PhoneVerified = PhoneVerified,
            DiscordUsername = DiscordUsername,
            DiscordVerified = DiscordVerified,
            PushNotificationIdentifier = PushNotificationIdentifier,
            DeviceType = DeviceType,
            MultiboxAccounts = MultiboxAccounts,
        };
    }

    public static DbUserModel FromRegistrationRequest(AccountRegistrationRequest request)
    {
        return new DbUserModel()
        {
            Username = request.Username,
            PasswordHash = JwtManager.HashString(request.Password),
            Email = request.Email,
            DeviceIdentifier = request.DeviceIdentifier,
            PhoneNumber = request.PhoneNumber,
        };
    }

    public bool HasClaim(UserClaim claim)
    {
        return Claims.Contains(claim);
    }
}

public class AccountBan
{
    public string Reason { get; set; }
    public string AdministratorNotes { get; set; }
    public DateTime DateExpires { get; set; }
    public DateTime DateApplied { get; set; } = DateTime.UtcNow;
}