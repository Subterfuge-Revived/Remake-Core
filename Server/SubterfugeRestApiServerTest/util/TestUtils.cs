using SubterfugeCore.Models.GameEvents;
using SubterfugeRestApiClient;

namespace SubterfugeServerConsole.Connections;

public class TestUtils
{

    public static MongoIntegrationTestConnector Mongo = new MongoIntegrationTestConnector();
    private static SubterfugeClient _client = null;
    public static SubterfugeClient GetClient()
    {
        if (_client == null)
        {
            string hostname = "localhost";
            int port = 5295;
            
            // Get environment
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
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
        var account = await TestUtils.Mongo.CreateTestingSuperUser();
        return await TestUtils.GetClient().UserApi.Login(new AuthorizationRequest() { Username = "admin", Password = "admin" });
    }
}