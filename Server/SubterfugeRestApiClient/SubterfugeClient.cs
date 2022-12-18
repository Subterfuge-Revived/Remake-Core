using System.Net.Http.Headers;
using SubterfugeRestApiClient.controllers.account;
using SubterfugeRestApiClient.controllers.game;
using SubterfugeRestApiClient.controllers.health;
using SubterfugeRestApiClient.controllers.social;
using SubterfugeRestApiClient.controllers.specialists;

namespace SubterfugeRestApiClient
{
    public class SubterfugeClient
    {
        private HttpClient Client = new HttpClient();
        
        // APIs
        public UserControllerClient UserApi;
        public UserRoleClient UserRoles;
        public GameEventClient GameEventClient;
        public GroupClient GroupClient;
        public LobbyClient LobbyClient;
        public HealthClient HealthClient;
        public SocialClient SocialClient;
        public SpecialistClient SpecialistClient;

        public SubterfugeClient()
        {
            string hostname = "localhost";
            int port = 5295;
            
            // Get environment
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environment == "Docker")
            {
                hostname = "server";
            }
            setupClient($"http://{hostname}:{port}/");
        }
        
        public SubterfugeClient(string baseUrl)
        {
            setupClient(baseUrl);
        }

        private void setupClient(string baseUrl)
        {
            Client.BaseAddress = new Uri(baseUrl);
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );

            UserApi = new UserControllerClient(Client);
            UserRoles = new UserRoleClient(Client);
            GameEventClient = new GameEventClient(Client);
            GroupClient = new GroupClient(Client);
            LobbyClient = new LobbyClient(Client);
            HealthClient = new HealthClient(Client);
            SocialClient = new SocialClient(Client);
            SpecialistClient = new SpecialistClient(Client);
        }
    }    
}