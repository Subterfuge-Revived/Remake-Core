using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Mime;
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
        static readonly HttpClient Client = new HttpClient();
        
        /// <summary>
        /// The API URL. The default value is set to "http://localhost/subterfuge-backend/sandbox/event_exec.php".
        /// You can override this URL by calling the API constuctor and passing in the URL manually. 
        /// </summary>
        private string _url = "http://localhost/";
        
        /// <summary>
        /// Once the user has logged in, their SESSION_ID token will be saved in the Api instance. This ensures
        /// that you don't need to keep track of the user's session token or send the token along if you are repeatedly
        /// using the same Api instance.
        /// </summary>
        private static string _sessionId = null;
        
        /// <summary>
        /// If the user has been authenticated.
        /// </summary>
        public bool IsAuthenticated { get; private set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Api()
        {
            // Dummy variable so that Newtonsoft.Json is packaged.
            IsAuthenticated = false;
        }

        /// <summary>
        /// Constructor that allows overriding the Api's URL
        /// </summary>
        /// <param name="url">The base URL to send requests to. This should point to the `exec_event.php` file.</param>
        public Api(string url)
        {
            // Constructor to override the API's default url.
            this._url = url;
        }

        /// <summary>
        /// If the user has already logged and you are creating a new instance of the Api, this method allows
        /// setting the user's token for future requests.
        /// </summary>
        /// <param name="token">The user's session token</param>
        public void SetToken(string token)
        {
            this.IsAuthenticated = true;
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
        public async Task<LoginResponse> Login(string username, string password)
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("player_name", username),
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("type", "login")
            });

            HttpResponseMessage response = await Client.PostAsync(_url, formContent);
            // Read the response
            string responseContent = await response.Content.ReadAsStringAsync();

            LoginResponse loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseContent);

            // If the login was successful, store the credentials in the Api object so that future calls don't need the token.
            if (loginResponse.Success)
            {
                this.IsAuthenticated = true;
                _sessionId = loginResponse.Token;
            }
            
            return loginResponse;
        }

        /// <summary>
        /// Gets a list of all open game rooms
        /// </summary>
        /// <returns>A list of open game rooms</returns>
        public async Task<List<GameRoom>> GetOpenRooms()
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("session_id", _sessionId),
                new KeyValuePair<string, string>("type", "get_room_data"),
                new KeyValuePair<string, string>("room_status", "open")
            });

            HttpResponseMessage response = await Client.PostAsync(_url, formContent);
            // Read the response
            string responseContent = await response.Content.ReadAsStringAsync();

            List<GameRoom> roomListResponse = JsonConvert.DeserializeObject<List<GameRoom>>(responseContent);
            return roomListResponse;
        }
        
        /// <summary>
        /// Gets a list of all ongoing rooms that the user is a member of
        /// </summary>
        /// <returns>A list of ongoing game rooms</returns>
        public async Task<List<GameRoom>> GetOngoingRooms()
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("session_id", _sessionId),
                new KeyValuePair<string, string>("type", "get_room_data"),
                new KeyValuePair<string, string>("room_status", "ongoing"),
                new KeyValuePair<string, string>("filter_player", "true")
            });

            HttpResponseMessage response = await Client.PostAsync(_url, formContent);
            // Read the response
            string responseContent = await response.Content.ReadAsStringAsync();

            List<GameRoom> roomListResponse = JsonConvert.DeserializeObject<List<GameRoom>>(responseContent);
            return roomListResponse;
        }

        /// <summary>
        /// Join a game room
        /// </summary>
        /// <param name="roomId">The room id of the game to join</param>
        /// <returns>The JoinLobbyResponse</returns>
        public async Task<JoinLobbyResponse> JoinLobby(int roomId)
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("session_id", _sessionId),
                new KeyValuePair<string, string>("type", "join_room"),
                new KeyValuePair<string, string>("room_id", roomId.ToString()),
            });

            HttpResponseMessage response = await Client.PostAsync(_url, formContent);
            // Read the response
            string responseContent = await response.Content.ReadAsStringAsync();

            JoinLobbyResponse roomListResponse = JsonConvert.DeserializeObject<JoinLobbyResponse>(responseContent);
            return roomListResponse;
        }
        
        /// <summary>
        /// Removes the player from the specified game lobby
        /// </summary>
        /// <param name="roomId">The room id to remove the player from</param>
        /// <returns>The LeaveLobbyResponse</returns>
        public async Task<LeaveLobbyResponse> LeaveLobby(int roomId)
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("session_id", _sessionId),
                new KeyValuePair<string, string>("type", "leave_room"),
                new KeyValuePair<string, string>("room_id", roomId.ToString()),
            });

            HttpResponseMessage response = await Client.PostAsync(_url, formContent);
            // Read the response
            string responseContent = await response.Content.ReadAsStringAsync();

            LeaveLobbyResponse roomListResponse = JsonConvert.DeserializeObject<LeaveLobbyResponse>(responseContent);
            return roomListResponse;
        }
        
        /// <summary>
        /// Starts a game early if the room's capacity is not filled.
        /// This action can only be performed by the creator of the lobby.
        /// </summary>
        /// <param name="roomId">The room id to start early.</param>
        /// <returns>the StartLobbyEarlyResponse</returns>
        public async Task<StartLobbyEarlyResponse> StartLobbyEarly(int roomId)
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("session_id", _sessionId),
                new KeyValuePair<string, string>("type", "start_early"),
                new KeyValuePair<string, string>("room_id", roomId.ToString()),
            });

            HttpResponseMessage response = await Client.PostAsync(_url, formContent);
            // Read the response
            string responseContent = await response.Content.ReadAsStringAsync();

            StartLobbyEarlyResponse roomListResponse = JsonConvert.DeserializeObject<StartLobbyEarlyResponse>(responseContent);
            return roomListResponse;
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
        public async Task<CreateLobbyResponse> CreateLobby(string title, int maxPlayers, int minRating, bool rated, bool anonymous, int goal, int map)
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

            HttpResponseMessage response = await Client.PostAsync(_url, formContent);
            // Read the response
            string responseContent = await response.Content.ReadAsStringAsync();

            CreateLobbyResponse roomListResponse = JsonConvert.DeserializeObject<CreateLobbyResponse>(responseContent);
            return roomListResponse;
        }

        /// <summary>
        /// Submits a game event to the a game room. Note: The player must be in the game room
        /// to be able to submit an event to it and the GameEvent must be a valid game event.
        /// </summary>
        /// <param name="gameEvent">The GameEvent to submit to the server</param>
        /// <param name="gameRoom">The id of the game room to submit an event to.</param>
        /// <returns>The SubmitEventResponse</returns>
        public async Task<SubmitEventResponse> SubmitGameEvent(GameEvent gameEvent, int gameRoom)
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("session_id", _sessionId),
                new KeyValuePair<string, string>("type", "submit_event"),
                new KeyValuePair<string, string>("room_id", gameRoom.ToString()),
                new KeyValuePair<string, string>("occurs_at", gameEvent.GetTick().GetTick().ToString()),
                new KeyValuePair<string, string>("event_msg", gameEvent.ToJson()),
            });

            HttpResponseMessage response = await Client.PostAsync(_url, formContent);
            // Read the response
            string responseContent = await response.Content.ReadAsStringAsync();

            SubmitEventResponse submitEventResponse = JsonConvert.DeserializeObject<SubmitEventResponse>(responseContent);
            return submitEventResponse;
        }
        
        /// <summary>
        /// Gets a list of all of the GameEvents for the specified gameroom
        /// </summary>
        /// <param name="gameRoom">The id of the game room to fetch events for.</param>
        /// <returns>A list of game events</returns>
        public async Task<List<GameEvent>> GetGameEvents(int gameRoom)
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("session_id", _sessionId),
                new KeyValuePair<string, string>("type", "get_events"),
                new KeyValuePair<string, string>("room_id", gameRoom.ToString()),
            });

            HttpResponseMessage response = await Client.PostAsync(_url, formContent);
            // Read the response
            string responseContent = await response.Content.ReadAsStringAsync();

            List<NetworkGameEvent> gameEventResponse = JsonConvert.DeserializeObject<List<NetworkGameEvent>>(responseContent);
            List<GameEvent> gameEvents = new List<GameEvent>();
            
            // Parse network game events to game events.
            foreach(NetworkGameEvent gameEvent in gameEventResponse)
            {
                gameEvents.Add(LaunchEvent.FromJson(gameEvent.EventMsg));
            }
            
            return gameEvents;
        }

        /// <summary>
        /// Register a new account.
        /// </summary>
        /// <param name="username">The username to register</param>
        /// <param name="password">The password to register</param>
        /// <param name="email">The email address to register</param>
        /// <returns>The RegisterResponse</returns>
        public async Task<RegisterResponse> RegisterAccount(string username, string password, string email)
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("type", "register"),
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("email", email),
            });

            HttpResponseMessage response = await Client.PostAsync(_url, formContent);
            // Read the response
            string responseContent = await response.Content.ReadAsStringAsync();

            RegisterResponse registerResponse = JsonConvert.DeserializeObject<RegisterResponse>(responseContent);

            if (registerResponse.Success)
            {
                _sessionId = registerResponse.Token;
                IsAuthenticated = true;
            }
            
            return registerResponse;
        }
    }
}