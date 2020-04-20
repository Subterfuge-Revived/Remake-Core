using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SubterfugeCore.Core.GameEvents;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.Players;

namespace SubterfugeCore.Core.Network
{
    
    /// <summary>
    /// API class to communicate with the backend API
    /// </summary>
    public class Api
    {
        /// <summary>
        /// Http client to send and recieve http requests
        /// </summary>
        static HttpClient Client = new HttpClient();
        
        /// <summary>
        /// The API URL. The default value is set to "http://localhost/subterfuge-backend/sandbox/event_exec.php".
        /// You can override this URL by calling the API constuctor and passing in the URL manually. 
        /// </summary>
        public static string Url { get; set; } = "http://localhost/";

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
        public Api()
        {
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Constructor that allows overriding the Api's URL
        /// </summary>
        /// <param name="url">The base URL to send requests to. This should point to the `exec_event.php` file.</param>
        public Api(string url)
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
                new KeyValuePair<string, string>("type", "login")
            });

            HttpResponseMessage response = await Client.PostAsync(Url, formContent);
            NetworkResponse<LoginResponse> loginResponse = await NetworkResponse<LoginResponse>.FromHttpResponse(response);

            if (loginResponse.IsSuccessStatusCode())
            {
                _sessionId = loginResponse.Response.Token;
            }

            return loginResponse;
        }

        /// <summary>
        /// Gets a list of all open game rooms
        /// </summary>
        /// <returns>A list of open game rooms</returns>
        public async Task<NetworkResponse<List<GameRoom>>> GetOpenRooms()
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("session_id", _sessionId),
                new KeyValuePair<string, string>("type", "get_room_data"),
                new KeyValuePair<string, string>("room_status", "open")
            });

            HttpResponseMessage response = await Client.PostAsync(Url, formContent);
            return await NetworkResponse<List<GameRoom>>.FromHttpResponse(response);
        }
        
        /// <summary>
        /// Gets a list of all ongoing rooms that the user is a member of
        /// </summary>
        /// <returns>A list of ongoing game rooms</returns>
        public async Task<NetworkResponse<List<GameRoom>>> GetOngoingRooms()
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("session_id", _sessionId),
                new KeyValuePair<string, string>("type", "get_room_data"),
                new KeyValuePair<string, string>("room_status", "ongoing"),
                new KeyValuePair<string, string>("filter_player", "true")
            });

            HttpResponseMessage response = await Client.PostAsync(Url, formContent);
            return await NetworkResponse<List<GameRoom>>.FromHttpResponse(response);
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
                new KeyValuePair<string, string>("room_id", roomId.ToString()),
                new KeyValuePair<string, string>("type", "join_room")
            });

            HttpResponseMessage response = await Client.PostAsync(Url, formContent);
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
                new KeyValuePair<string, string>("type", "leave_room"),
                new KeyValuePair<string, string>("room_id", roomId.ToString()),
            });
            
            HttpResponseMessage response = await Client.PostAsync(Url, formContent);
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
                new KeyValuePair<string, string>("type", "start_early"),
                new KeyValuePair<string, string>("room_id", roomId.ToString()),
            });

            HttpResponseMessage response = await Client.PostAsync(Url, formContent);
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
        public async Task<NetworkResponse<CreateLobbyResponse>> CreateLobby(string title, int maxPlayers, int minRating, bool rated, bool anonymous, int goal, int map)
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("session_id", _sessionId),
                new KeyValuePair<string, string>("type", "new_room"),
                new KeyValuePair<string, string>("description", title),
                new KeyValuePair<string, string>("max_players", maxPlayers.ToString()),
                new KeyValuePair<string, string>("min_rating", minRating.ToString()),
                new KeyValuePair<string, string>("rated", rated.ToString()),
                new KeyValuePair<string, string>("anonymity", anonymous.ToString()),
                new KeyValuePair<string, string>("goal", goal.ToString()),
                new KeyValuePair<string, string>("map", map.ToString()),
            });

            HttpResponseMessage response = await Client.PostAsync(Url, formContent);
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
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("session_id", _sessionId),
                new KeyValuePair<string, string>("type", "submit_event"),
                new KeyValuePair<string, string>("room_id", gameRoom.ToString()),
                new KeyValuePair<string, string>("occurs_at", gameEvent.GetTick().GetTick().ToString()),
                new KeyValuePair<string, string>("event_msg", gameEvent.ToJson()),
            });

            HttpResponseMessage response = await Client.PostAsync(Url, formContent);
            return await NetworkResponse<SubmitEventResponse>.FromHttpResponse(response);
        }
        
        /// <summary>
        /// Gets a list of all of the GameEvents for the specified gameroom
        /// </summary>
        /// <param name="gameRoom">The id of the game room to fetch events for.</param>
        /// <returns>A list of game events</returns>
        public async Task<NetworkResponse<List<NetworkGameEvent>>> GetGameEvents(int gameRoom)
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("session_id", _sessionId),
                new KeyValuePair<string, string>("type", "get_events"),
                new KeyValuePair<string, string>("room_id", gameRoom.ToString()),
            });
            
            
            HttpResponseMessage response = await Client.PostAsync(Url, formContent);
            return await NetworkResponse<List<NetworkGameEvent>>.FromHttpResponse(response);
        }

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
                new KeyValuePair<string, string>("type", "register"),
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("email", email),
            });
            
            HttpResponseMessage response = await Client.PostAsync(Url, formContent);
            return await NetworkResponse<RegisterResponse>.FromHttpResponse(response);
        }

        public async Task<NetworkResponse<List<NetworkMessage>>> GetGroupMessages(int GroupNumber)
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("type", "get_message"),
                new KeyValuePair<string, string>("session_id", _sessionId),
                new KeyValuePair<string, string>("group_id", GroupNumber.ToString()),
            });

            HttpResponseMessage response = await Client.PostAsync(Url, formContent);
            return await NetworkResponse<List<NetworkMessage>>.FromHttpResponse(response);
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
                new KeyValuePair<string, string>("type", "create_group"),
                new KeyValuePair<string, string>("session_id", _sessionId),
                new KeyValuePair<string, string>("room_id", gameRoom.ToString()),
                new KeyValuePair<string, string>("participants[]", playerIds),
            });
        
            HttpResponseMessage response = await Client.PostAsync(Url, formContent);
            return await NetworkResponse<CreateGroupResponse>.FromHttpResponse(response);
        }
        
        public async Task<NetworkResponse<SendMessageResponse>> SendMessage(int groupId, string message)
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("type", "message"),
                new KeyValuePair<string, string>("session_id", _sessionId),
                new KeyValuePair<string, string>("group_id", groupId.ToString()),
                new KeyValuePair<string, string>("message", message),
            });
        
            HttpResponseMessage response = await Client.PostAsync(Url, formContent);
            return await NetworkResponse<SendMessageResponse>.FromHttpResponse(response);
        }

        public async Task<NetworkResponse<BlockPlayerResponse>> BlockPlayer(Player player)
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("type", "block"),
                new KeyValuePair<string, string>("session_id", _sessionId),
                new KeyValuePair<string, string>("other_player_id", player.GetId().ToString()),
            });
        
            HttpResponseMessage response = await Client.PostAsync(Url, formContent);
            return await NetworkResponse<BlockPlayerResponse>.FromHttpResponse(response);
        }

        public async Task<NetworkResponse<UnblockPlayerResponse>> UnblockPlayer(Player player)
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("type", "unblock"),
                new KeyValuePair<string, string>("session_id", _sessionId),
                new KeyValuePair<string, string>("other_player_id", player.GetId().ToString()),
            });
        
            HttpResponseMessage response = await Client.PostAsync(Url, formContent);
            return await NetworkResponse<UnblockPlayerResponse>.FromHttpResponse(response);
        }

        public async Task<NetworkResponse<List<BlockedPlayer>>> GetBlockList()
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("type", "get_blocks"),
                new KeyValuePair<string, string>("session_id", _sessionId),
            });
        
            HttpResponseMessage response = await Client.PostAsync(Url, formContent);
            return await NetworkResponse<List<BlockedPlayer>>.FromHttpResponse(response);
        }
        
    }
}