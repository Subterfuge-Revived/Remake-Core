#nullable enable
using Subterfuge.Remake.Api.Client;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Server.Database;

namespace Subterfuge.Remake.Server.Test.util;

public class TestUtils
{
    public static MongoConnector Mongo = new MongoIntegrationTestConnector().Mongo;
    private static SubterfugeClient? _client = null;
    public static SubterfugeClient GetClient()
    {
        if (_client == null)
        {
            string hostname = "localhost";
            int port = 5295;
            
            // Get environment
            string? environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environment == "Docker")
            {
                hostname = "server";
            }
            _client = new SubterfugeClient($"http://{hostname}:{port}");
            return _client;
        }

        return _client;
    }

    public static async Task<AuthorizationResponse> CreateSuperUserAndLogin()
    {
        var account = await Mongo.CreateSuperUser();
        return (await TestUtils.GetClient().UserApi.Login(new AuthorizationRequest() { Username = "admin", Password = "admin" })).GetOrThrow();
    }

    public static JoinRoomRequest CreateJoinRequest()
    {
        return new JoinRoomRequest()
        {
            SpecialistDeck = new List<SpecialistIds>()
            {
                SpecialistIds.Advisor,
                SpecialistIds.Amnesiac,
                SpecialistIds.Assasin,
                SpecialistIds.Automation,
                SpecialistIds.Dispatcher,
                SpecialistIds.Economist,
                SpecialistIds.Enforcer,
                SpecialistIds.Engineer,
                SpecialistIds.Escort,
                SpecialistIds.Foreman,
                SpecialistIds.Helmsman,
                SpecialistIds.Hypnotist,
                SpecialistIds.Icicle,
                SpecialistIds.Industrialist,
                SpecialistIds.Infiltrator
            }
        };
    }
}