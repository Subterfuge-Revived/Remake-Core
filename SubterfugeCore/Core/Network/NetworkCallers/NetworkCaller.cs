using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.Players;

namespace SubterfugeCore.Core.Network
{
    public class NetworkCaller : INetworkCaller
    {
        /// <summary>
        /// Http client to send and recieve http requests
        /// </summary>
        static HttpClient Client = new HttpClient();
        
        /// <summary>
        /// The API URL. The default value is set to "http://localhost".
        /// You can override this URL by calling the API constuctor and passing in the URL manually.
        /// This URL should NOT include a trailing slash. 
        /// </summary>
        public static string Url { get; set; } = "http://localhost";

        /// <summary>
        /// Once the user has logged in, their SESSION_ID token will be saved in the Api instance. This ensures
        /// that you don't need to keep track of the user's session token or send the token along if you are repeatedly
        /// using the same Api instance.
        /// </summary>
        private static string _sessionId = null;
        
        // private CookieContainer _cookieContainer = new CookieContainer();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public NetworkCaller()
        {
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Constructor that allows overriding the Api's URL
        /// </summary>
        /// <param name="url">The base URL to send requests to. This should point to the `exec_event.php` file.</param>
        public NetworkCaller(string url)
        {
            // Constructor to override the API's default url.
            Url = url;
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// If the user has already logged and you are creating a new instance of the Api, this method allows
        /// setting the user's token for future requests.
        /// </summary>
        /// <param name="token">The user's session token</param>
        public void SetToken(string token)
        {
            _sessionId = token;
        }
        
         /////////////////////////////////////////////////////////////////
        //
        // Authentication Methods
        //
        /////////////////////////////////////////////////////////////////

        /// <summary>
        /// Register a new account.
        /// </summary>
        /// <param name="username">The username to register</param>
        /// <param name="password">The password to register</param>
        /// <param name="email">The email address to register</param>
        /// <returns>The RegisterResponse</returns>
        public async Task<NetworkResponse<RegisterResponse>> RegisterAccount(string username, string password, string email)
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("email", email),
            });

            HttpResponseMessage response = await _sendRequest(HttpMethod.Post, Url + "/api/register", formContent);
            return await NetworkResponse<RegisterResponse>.FromHttpResponse(response);
        }
        
        /// <summary>
        /// Logs the user into the API. If successful, stores the user's session token
        /// in the Api object so that future requests don't need to accept the token.
        /// Note: creating a `new Api()` instance will have the user's token persist.
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <param name="password">The user's password</param>
        /// <returns>The login response</returns>
        public async Task<NetworkResponse<LoginResponse>> Login(string username, string password)
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password),
            });

            HttpResponseMessage response = await _sendRequest(HttpMethod.Post, Url + "/api/login", formContent);
            NetworkResponse<LoginResponse> loginResponse = await NetworkResponse<LoginResponse>.FromHttpResponse(response);

            if (loginResponse.IsSuccessStatusCode())
            {
                _sessionId = loginResponse.Response.token;
            }

            return loginResponse;
        }
        
        /////////////////////////////////////////////////////////////////
        //
        // GameRoom Methods
        //
        /////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets a list of all open game rooms
        /// </summary>
        /// <returns>A list of open game rooms</returns>
        public async Task<NetworkResponse<GameRoomResponse>> GetOpenRooms()
        {
            string getEndpoint = $"{Url}/api/rooms?session_id={_sessionId}&room_status=open";

            HttpResponseMessage response = await _sendRequest(HttpMethod.Get, getEndpoint, null);
            return await NetworkResponse<GameRoomResponse>.FromHttpResponse(response);
        }
        
        /// <summary>
        /// Gets a list of all ongoing rooms that the user is a member of
        /// </summary>
        /// <returns>A list of ongoing game rooms</returns>
        public async Task<NetworkResponse<GameRoomResponse>> GetOngoingRooms()
        {
            string getEndpoint = $"{Url}/api/rooms?session_id={_sessionId}&room_status=ongoing&filter_player=true";

            HttpResponseMessage response = await _sendRequest(HttpMethod.Get, getEndpoint, null);
            return await NetworkResponse<GameRoomResponse>.FromHttpResponse(response);
        }

        /// <summary>
        /// Join a game room
        /// </summary>
        /// <param name="roomId">The room id of the game to join</param>
        /// <returns>The JoinLobbyResponse</returns>
        public async Task<NetworkResponse<JoinLobbyResponse>> JoinLobby(int roomId)
        {
            
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("session_id", _sessionId),
            });

            HttpResponseMessage response =
                await _sendRequest(HttpMethod.Post, $"{Url}/api/rooms/{roomId}/join", formContent);
            
            return await NetworkResponse<JoinLobbyResponse>.FromHttpResponse(response);
        }
        
        /// <summary>
        /// Removes the player from the specified game lobby
        /// </summary>
        /// <param name="roomId">The room id to remove the player from</param>
        /// <returns>The LeaveLobbyResponse</returns>
        public async Task<NetworkResponse<LeaveLobbyResponse>> LeaveLobby(int roomId)
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("session_id", _sessionId),
            });

            HttpResponseMessage response =
                await _sendRequest(HttpMethod.Post, $"{Url}/api/rooms/{roomId}/leave", formContent);
            
            return await NetworkResponse<LeaveLobbyResponse>.FromHttpResponse(response);
        }
        
        /// <summary>
        /// Starts a game early if the room's capacity is not filled.
        /// This action can only be performed by the creator of the lobby.
        /// </summary>
        /// <param name="roomId">The room id to start early.</param>
        /// <returns>the StartLobbyEarlyResponse</returns>
        public async Task<NetworkResponse<StartLobbyEarlyResponse>> StartLobbyEarly(int roomId)
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("session_id", _sessionId),
            });

            HttpResponseMessage response =
                await _sendRequest(HttpMethod.Post, $"{Url}/api/rooms/{roomId}/start", formContent);
            
            return await NetworkResponse<StartLobbyEarlyResponse>.FromHttpResponse(response);
        }
        
        /// <summary>
        /// Creates a new game lobby
        /// </summary>
        /// <param name="title">The title of the lobby</param>
        /// <param name="max_players">The maximum number of players</param>
        /// <param name="min_rating">The minimum rating</param>
        /// <param name="rated">If the game is a ranked game</param>
        /// <param name="anonymous">If the game is anonymous</param>
        /// <param name="goal">The goal of the game</param>
        /// <param name="map">The map that the game is played on</param>
        /// <returns>the CreateLobbyResponse</returns>
        public async Task<NetworkResponse<CreateLobbyResponse>> CreateLobby(string title, int maxPlayers, int minRating, bool rated, bool anonymous, string goal, int map)
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("session_id", _sessionId),
                new KeyValuePair<string, string>("description", title),
                new KeyValuePair<string, string>("max_players", maxPlayers.ToString()),
                new KeyValuePair<string, string>("min_rating", minRating.ToString()),
                new KeyValuePair<string, string>("rated", rated ? "1" : "0"),
                new KeyValuePair<string, string>("anonymity", anonymous ? "1" : "0"),
                new KeyValuePair<string, string>("goal", goal),
                new KeyValuePair<string, string>("map", map.ToString()),
            });

            HttpResponseMessage response = await _sendRequest(HttpMethod.Post, $"{Url}/api/rooms", formContent);
            return await NetworkResponse<CreateLobbyResponse>.FromHttpResponse(response);
        }

        /// <summary>
        /// Submits a game event to the a game room. Note: The player must be in the game room
        /// to be able to submit an event to it and the GameEvent must be a valid game event.
        /// </summary>
        /// <param name="gameEvent">The GameEvent to submit to the server</param>
        /// <param name="gameRoom">The id of the game room to submit an event to.</param>
        /// <returns>The SubmitEventResponse</returns>
        public async Task<NetworkResponse<SubmitEventResponse>> SubmitGameEvent(GameEvent gameEvent, int gameRoom)
        {
            
            Int32 unixTimestamp = (Int32)(gameEvent.GetTick().GetDate().Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("session_id", _sessionId),
                new KeyValuePair<string, string>("occurs_at", unixTimestamp.ToString()),
                new KeyValuePair<string, string>("event_msg", '"' + gameEvent.ToJson() + '"'),
            });

            HttpResponseMessage response =
                await _sendRequest(HttpMethod.Post, $"{Url}/api/rooms/{gameRoom}/events", formContent);
            
            return await NetworkResponse<SubmitEventResponse>.FromHttpResponse(response);
        }

        /// <summary>
        /// Gets a list of all of the GameEvents for the specified gameroom
        /// </summary>
        /// <param name="gameRoom">The id of the game room to fetch events for.</param>
        /// <returns>A list of game events</returns>
        public async Task<NetworkResponse<GameEventResponse>> GetGameEvents(int gameRoom)
        {
            string getEndpoint = $"{Url}/api/rooms/{gameRoom}/events?session_id={_sessionId}&filter=tick&filter_arg=0";

            HttpResponseMessage response =
                await _sendRequest(HttpMethod.Get, getEndpoint, null);
            
            return await NetworkResponse<GameEventResponse>.FromHttpResponse(response);
        }

        /////////////////////////////////////////////////////////////////
        //
        // Messaging Methods
        //
        /////////////////////////////////////////////////////////////////

        public async Task<NetworkResponse<GroupMessageListResponse>> GetGroupMessages(int gameRoom, int GroupNumber)
        {
            string getEndpoint = $"{Url}/api/rooms/{gameRoom}/groups/{GroupNumber}/messages?session_id={_sessionId}";

            HttpResponseMessage response = await _sendRequest(HttpMethod.Get, getEndpoint, null);
            return await NetworkResponse<GroupMessageListResponse>.FromHttpResponse(response);
        }


        public async Task<NetworkResponse<CreateGroupResponse>> CreateGroup(int gameRoom, List<Player> players)
        {
            // Concatenate all the player IDs
            string playerIds = "";
            foreach(Player p in players)
            {
                playerIds += p.GetId().ToString() + ",";
            }

            // Trim the last comma
            playerIds.Substring(0, playerIds.Length - 1);
            
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("session_id", _sessionId),
                new KeyValuePair<string, string>("room_id", gameRoom.ToString()),
                new KeyValuePair<string, string>("participants[]", playerIds),
            });

            HttpResponseMessage response =
                await _sendRequest(HttpMethod.Post, $"{Url}/api/rooms/{gameRoom}/groups", formContent);
            
            return await NetworkResponse<CreateGroupResponse>.FromHttpResponse(response);
        }
        
        public async Task<NetworkResponse<SendMessageResponse>> SendMessage(int gameRoom, int groupId, string message)
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("session_id", _sessionId),
                new KeyValuePair<string, string>("message", message),
            });

            HttpResponseMessage response = await _sendRequest(HttpMethod.Post,
                $"{Url}/api/rooms/{gameRoom}/groups/{groupId}/messages", formContent);
            
            return await NetworkResponse<SendMessageResponse>.FromHttpResponse(response);
        }
        
        /////////////////////////////////////////////////////////////////
        //
        // Social Methods
        //
        /////////////////////////////////////////////////////////////////

        public async Task<NetworkResponse<BlockPlayerResponse>> BlockPlayer(Player player)
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("session_id", _sessionId),
                new KeyValuePair<string, string>("other_player_id", player.GetId().ToString()),
            });

            HttpResponseMessage response = await _sendRequest(HttpMethod.Post, $"{Url}/api/blocks", formContent);
            return await NetworkResponse<BlockPlayerResponse>.FromHttpResponse(response);
        }

        public async Task<NetworkResponse<UnblockPlayerResponse>> UnblockPlayer(int BlockId)
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("session_id", _sessionId),
            });

            HttpResponseMessage response =
                await _sendRequest(HttpMethod.Delete, $"{Url}/api/blocks/{BlockId}", formContent);
            
            return await NetworkResponse<UnblockPlayerResponse>.FromHttpResponse(response);
        }

        public async Task<NetworkResponse<BlockPlayerResponse>> GetBlockList()
        {   
            string getEndpoint = $"{Url}/api/blocks?session_id={_sessionId}";

            HttpResponseMessage response = await _sendRequest(HttpMethod.Get, getEndpoint, null);
            return await NetworkResponse<BlockPlayerResponse>.FromHttpResponse(response);
        }

        private async Task<HttpResponseMessage> _sendRequest(HttpMethod method, String url,
            FormUrlEncodedContent content)
        {
            HttpRequestMessage message = new HttpRequestMessage(method, url);
            message.Content = content;

            HttpResponseMessage response = await Client.SendAsync(message);
            
            return response;
        }
    }
}