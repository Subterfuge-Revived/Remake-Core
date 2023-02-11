using SubterfugeCore.Models.GameEvents;
using SubterfugeRestApiServer.Authentication;

namespace SubterfugeDatabaseProvider.Models;

public class DbUserModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    // Administrative
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime BannedUntil { get; set; } = DateTime.MinValue;

    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string DeviceIdentifier { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string PhoneVerificationCode { get; set; } = "";
    public string DiscordUsername { get; set; } = "";
    public string DiscordVerificationCode { get; set; } = "";
    public UserClaim[] Claims { get; set; } = new[] { UserClaim.User };
    public string PushNotificationIdentifier { get; set; } = "";

    public string DeviceType { get; set; } = "";
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

    public SimpleUser ToSimpleUser()
    {
        return new SimpleUser()
        {
            Id = Id,
            Username = Username,
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
            DeviceIdentifier = JwtManager.HashString(DeviceIdentifier),
            PhoneNumberHash = JwtManager.HashString(PhoneNumber),
            DiscordUsername = DiscordUsername,
            PushNotificationIdentifier = PushNotificationIdentifier,
            DeviceType = DeviceType,
            MultiboxAccounts = MultiboxAccounts,
            BanHistory = BanHistory,
        };
    }

    public static DbUserModel FromRegistrationRequest(AccountRegistrationRequest request)
    {
        return new DbUserModel()
        {
            Username = request.Username,
            PasswordHash = JwtManager.HashString(request.Password),
            DeviceIdentifier = request.DeviceIdentifier,
            PhoneNumber = request.PhoneNumber,
        };
    }

    public bool HasClaim(UserClaim claim)
    {
        return Claims.Contains(claim);
    }
}