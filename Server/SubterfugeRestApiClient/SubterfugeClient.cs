using Subterfuge.Remake.Api.Client.controllers.account;
using Subterfuge.Remake.Api.Client.controllers.admin;
using Subterfuge.Remake.Api.Client.controllers.Client;
using Subterfuge.Remake.Api.Client.controllers.game;
using Subterfuge.Remake.Api.Client.controllers.health;
using Subterfuge.Remake.Api.Client.controllers.social;

namespace Subterfuge.Remake.Api.Client
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
            UserApi = new UserControllerClient(subsHttpClient);
            UserRoles = new UserRoleClient(subsHttpClient);
            GameEventClient = new GameEventClient(subsHttpClient);
            GroupClient = new GroupClient(subsHttpClient);
            LobbyClient = new LobbyClient(subsHttpClient);
            HealthClient = new HealthClient(subsHttpClient);
            SocialClient = new SocialClient(subsHttpClient);
            AdminClient = new AdminClient(subsHttpClient);
            AnnouncementClient = new GameAnnouncementClient(subsHttpClient);
        }
        
        /// <summary>
        /// Creates an instance of a subterfuge client using the specified baseURL.
        /// </summary>
        /// <param name="baseUrl">The server URL to connect to. Should include the protocol, hostname, and port. ex. http://localhost:5295</param>
        public SubterfugeClient(string baseUrl)
        {
            subsHttpClient = new SubterfugeHttpClient(baseUrl);
            UserApi = new UserControllerClient(subsHttpClient);
            UserRoles = new UserRoleClient(subsHttpClient);
            GameEventClient = new GameEventClient(subsHttpClient);
            GroupClient = new GroupClient(subsHttpClient);
            LobbyClient = new LobbyClient(subsHttpClient);
            HealthClient = new HealthClient(subsHttpClient);
            SocialClient = new SocialClient(subsHttpClient);
            AdminClient = new AdminClient(subsHttpClient);
            AnnouncementClient = new GameAnnouncementClient(subsHttpClient);
        }
    }    
}