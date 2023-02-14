using System.Net.Http.Headers;
using SubterfugeRestApiClient.controllers.account;
using SubterfugeRestApiClient.controllers.admin;
using SubterfugeRestApiClient.controllers.Client;
using SubterfugeRestApiClient.controllers.game;
using SubterfugeRestApiClient.controllers.health;
using SubterfugeRestApiClient.controllers.social;
using SubterfugeRestApiClient.controllers.specialists;

namespace SubterfugeRestApiClient
{
    public class SubterfugeClient
    {
        private SubterfugeHttpClient subsHttpClient;
        
        // APIs
        public UserControllerClient UserApi;
        public UserRoleClient UserRoles;
        public GameEventClient GameEventClient;
        public GroupClient GroupClient;
        public LobbyClient LobbyClient;
        public HealthClient HealthClient;
        public SocialClient SocialClient;
        public SpecialistClient SpecialistClient;
        public AdminClient AdminClient;
        public GameAnnouncementClient AnnouncementClient;
        
        /// <summary>
        /// The client constructor.
        ///
        /// By default, this constructor works for local development as well as docker container environments.
        /// The client will look for the `ASPNETCORE_ENVIRONMENT` environment variable.
        /// If the environment variable is `Docker`, the client connects to: "server:5295", otherwise it will connect to "localhost:5295".
        /// If you need to connect to a custom server URL, use the secondary constructor which accepts a base URL.
        /// </summary>
        public SubterfugeClient()
        {
            subsHttpClient = new SubterfugeHttpClient();
            WireDependencies();
        }
        
        /// <summary>
        /// Creates an instance of a subterfuge client using the specified baseURL.
        /// </summary>
        /// <param name="baseUrl">The server URL to connect to. Should include the protocol, hostname, and port. ex. http://localhost:5295</param>
        public SubterfugeClient(string baseUrl)
        {
            subsHttpClient = new SubterfugeHttpClient(baseUrl);
            WireDependencies();
        }

        private void WireDependencies()
        {
            UserApi = new UserControllerClient(subsHttpClient);
            UserRoles = new UserRoleClient(subsHttpClient);
            GameEventClient = new GameEventClient(subsHttpClient);
            GroupClient = new GroupClient(subsHttpClient);
            LobbyClient = new LobbyClient(subsHttpClient);
            HealthClient = new HealthClient(subsHttpClient);
            SocialClient = new SocialClient(subsHttpClient);
            SpecialistClient = new SpecialistClient(subsHttpClient);
            AdminClient = new AdminClient(subsHttpClient);
            AnnouncementClient = new GameAnnouncementClient(subsHttpClient);
        }
    }    
}