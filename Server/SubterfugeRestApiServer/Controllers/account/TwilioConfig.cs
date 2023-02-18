namespace Subterfuge.Remake.Server.Controllers.account;

public class TwilioConfig
{
    public readonly bool enabled;
    public readonly string twilioAccountSid;
    public readonly string twilioAuthToken;
    public readonly string twilioVerificationServiceSid;
    
    public TwilioConfig(IConfiguration phoneVerificationConfigSection)
    {
        enabled = Boolean.Parse(phoneVerificationConfigSection["Enabled"]);
        
        var twilioConfig = phoneVerificationConfigSection.GetSection("Twilio");
        twilioAccountSid = twilioConfig["AccountSid"];
        twilioAuthToken = twilioConfig["AuthToken"];
        twilioVerificationServiceSid = twilioConfig["VerificationServiceSid"];
    }
}