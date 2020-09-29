using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.Players;

namespace SubterfugeCore.Core.Network
{
    public class OfflineNetworkCaller : INetworkCaller
    {

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
        public OfflineNetworkCaller()
        {
            
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
            string stringContent = @"{""player"": {""id"": 1,""name"": ""asdfg""},""token"": ""GPe4PD3ZAaS0xVw3yXeLYnjHSN55qF64T2s0tOzvUGQ3DAkQDPaDoWVSPME1TAeeg6fSBHegGO4SOcgr""}";
            
            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
            message.Content = new StringContent(stringContent);
            return await NetworkResponse<RegisterResponse>.FromHttpResponse(message);
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
            string stringContent = @"{""player"": {""id"": 1,""name"": ""asdfg""},""token"": ""GPe4PD3ZAaS0xVw3yXeLYnjHSN55qF64T2s0tOzvUGQ3DAkQDPaDoWVSPME1TAeeg6fSBHegGO4SOcgr""}";
            
            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
            message.Content = new StringContent(stringContent);
            return await NetworkResponse<LoginResponse>.FromHttpResponse(message);
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
            string stringContent = @"[ { ""room_id"": 1, ""status"": ""open"", ""creator_id"": 1, ""rated"": 1, ""min_rating"": 0, ""description"": ""test"", ""goal"": 1, ""anonymity"": 1, ""map"": 0, ""seed"": 1601229023, ""started_at"": null, ""max_players"": 4, ""players"": [ { ""name"": ""Anonymous"", ""id"": 1 }, { ""name"": ""Anonymous"", ""id"": 2 }, { ""name"": ""Anonymous"", ""id"": 3 } ], ""message_groups"": [] }, { ""room_id"": 2, ""status"": ""open"", ""creator_id"": 1, ""rated"": 1, ""min_rating"": 0, ""description"": ""test"", ""goal"": 1, ""anonymity"": 1, ""map"": 0, ""seed"": 1601230013, ""started_at"": null, ""max_players"": 3, ""players"": [ { ""name"": ""Anonymous"", ""id"": 1 }, { ""name"": ""Anonymous"", ""id"": 3 } ], ""message_groups"": [] } ]";
            
            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
            message.Content = new StringContent(stringContent);
            return await NetworkResponse<GameRoomResponse>.FromHttpResponse(message);
        }
        
        /// <summary>
        /// Gets a list of all ongoing rooms that the user is a member of
        /// </summary>
        /// <returns>A list of ongoing game rooms</returns>
        public async Task<NetworkResponse<GameRoomResponse>> GetOngoingRooms()
        {
            string stringContent = @"[ { ""room_id"": 1, ""status"": ""open"", ""creator_id"": 1, ""rated"": 1, ""min_rating"": 0, ""description"": ""test"", ""goal"": 1, ""anonymity"": 1, ""map"": 0, ""seed"": 1601229023, ""started_at"": null, ""max_players"": 4, ""players"": [ { ""name"": ""Anonymous"", ""id"": 1 }, { ""name"": ""Anonymous"", ""id"": 2 }, { ""name"": ""Anonymous"", ""id"": 3 } ], ""message_groups"": [] }, { ""room_id"": 2, ""status"": ""open"", ""creator_id"": 1, ""rated"": 1, ""min_rating"": 0, ""description"": ""test"", ""goal"": 1, ""anonymity"": 1, ""map"": 0, ""seed"": 1601230013, ""started_at"": null, ""max_players"": 3, ""players"": [ { ""name"": ""Anonymous"", ""id"": 1 }, { ""name"": ""Anonymous"", ""id"": 3 } ], ""message_groups"": [] } ]";
            
            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
            message.Content = new StringContent(stringContent);
            return await NetworkResponse<GameRoomResponse>.FromHttpResponse(message);
        }

        /// <summary>
        /// Join a game room
        /// </summary>
        /// <param name="roomId">The room id of the game to join</param>
        /// <returns>The JoinLobbyResponse</returns>
        public async Task<NetworkResponse<JoinLobbyResponse>> JoinLobby(int roomId)
        {
            string stringContent = @"{ ""id"": 1, ""created_at"": ""2020-04-18 20:04:16"", ""updated_at"": ""2020-04-18 20:04:16"", ""started_at"": null, ""closed_at"": null, ""creator_player_id"": 1, ""goal_id"": 1, ""description"": ""test"", ""is_rated"": 0, ""is_anonymous"": 0, ""min_rating"": 0, ""max_players"": 4, ""map"": 0, ""seed"": 1587240256, ""players"": [ { ""id"": 1, ""created_at"": ""2020-04-18 19:59:43"", ""updated_at"": ""2020-04-18 19:59:43"", ""name"": ""asdfg"", ""email"": ""asdfg@asdfg.com"", ""rating"": 1200, ""wins"": 0, ""resignations"": 0, ""last_online_at"": ""2020-04-18 19:59:43"", ""pivot"": { ""room_id"": 1, ""player_id"": 1, ""id"": 1, ""created_at"": ""2020-04-18T20:04:16.000000Z"", ""updated_at"": ""2020-04-18T20:04:16.000000Z"" } }, { ""id"": 4, ""created_at"": ""2020-04-19 01:00:29"", ""updated_at"": ""2020-04-19 01:00:29"", ""name"": ""asdfg1"", ""email"": ""asdfg1@asdfg1.com"", ""rating"": 1200, ""wins"": 0, ""resignations"": 0, ""last_online_at"": ""2020-04-19 01:00:29"", ""pivot"": { ""room_id"": 1, ""player_id"": 4, ""id"": 2, ""created_at"": ""2020-04-19T01:00:45.000000Z"", ""updated_at"": ""2020-04-19T01:00:45.000000Z"" } } ] }";
            
            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
            message.Content = new StringContent(stringContent);
            return await NetworkResponse<JoinLobbyResponse>.FromHttpResponse(message);
        }
        
        /// <summary>
        /// Removes the player from the specified game lobby
        /// </summary>
        /// <param name="roomId">The room id to remove the player from</param>
        /// <returns>The LeaveLobbyResponse</returns>
        public async Task<NetworkResponse<LeaveLobbyResponse>> LeaveLobby(int roomId)
        {
            string stringContent = @"{ ""id"": 1, ""created_at"": ""2020-04-18 20:04:16"", ""updated_at"": ""2020-04-18 20:04:16"", ""started_at"": null, ""closed_at"": null, ""creator_player_id"": 1, ""goal_id"": 1, ""description"": ""test"", ""is_rated"": 0, ""is_anonymous"": 0, ""min_rating"": 0, ""max_players"": 4, ""map"": 0, ""seed"": 1587240256, ""players"": [] }";
            
            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
            message.Content = new StringContent(stringContent);
            return await NetworkResponse<LeaveLobbyResponse>.FromHttpResponse(message);
        }
        
        /// <summary>
        /// Starts a game early if the room's capacity is not filled.
        /// This action can only be performed by the creator of the lobby.
        /// </summary>
        /// <param name="roomId">The room id to start early.</param>
        /// <returns>the StartLobbyEarlyResponse</returns>
        public async Task<NetworkResponse<StartLobbyEarlyResponse>> StartLobbyEarly(int roomId)
        {            
            string stringContent = @"{ ""id"": 1, ""created_at"": ""2020-09-27 17:50:23"", ""updated_at"": ""2020-09-29 01:51:58"", ""started_at"": ""2020-09-29 01:51:58"", ""closed_at"": null, ""creator_player_id"": 1, ""goal_id"": 1, ""description"": ""test"", ""is_rated"": 1, ""is_anonymous"": 1, ""min_rating"": 0, ""max_players"": 4, ""map"": 0, ""seed"": 1601229023, ""creator_player"": { ""id"": 1, ""created_at"": ""2020-09-26 04:42:56"", ""updated_at"": ""2020-09-26 04:42:56"", ""name"": ""asdfg1"", ""rating"": 1200, ""wins"": 0, ""resignations"": 0, ""last_online_at"": ""2020-09-26 04:42:56"" }, ""players"": [ { ""id"": 1, ""created_at"": ""2020-09-26 04:42:56"", ""updated_at"": ""2020-09-26 04:42:56"", ""name"": ""asdfg1"", ""rating"": 1200, ""wins"": 0, ""resignations"": 0, ""last_online_at"": ""2020-09-26 04:42:56"", ""pivot"": { ""room_id"": 1, ""player_id"": 1, ""id"": 1, ""created_at"": ""2020-09-27T17:50:23.000000Z"", ""updated_at"": ""2020-09-27T17:50:23.000000Z"" } }, { ""id"": 2, ""created_at"": ""2020-09-27 18:04:06"", ""updated_at"": ""2020-09-27 18:04:06"", ""name"": ""asdfg2"", ""rating"": 1200, ""wins"": 0, ""resignations"": 0, ""last_online_at"": ""2020-09-27 18:04:06"", ""pivot"": { ""room_id"": 1, ""player_id"": 2, ""id"": 2, ""created_at"": ""2020-09-27T18:05:49.000000Z"", ""updated_at"": ""2020-09-27T18:05:49.000000Z"" } } ] }";
            
            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
            message.Content = new StringContent(stringContent);
            return await NetworkResponse<StartLobbyEarlyResponse>.FromHttpResponse(message);
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
            string stringContent = @"{ ""created_room"": { ""room_id"": 1, ""creator"": 1, ""description"": ""test"", ""rated"": false, ""max_players"": ""4"", ""player_count"": 1, ""min_rating"": 0, ""goal"": ""neptunium-200"", ""anonymity"": false, ""map"": ""0"", ""seed"": 1587240256 } }";
            
            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
            message.Content = new StringContent(stringContent);
            return await NetworkResponse<CreateLobbyResponse>.FromHttpResponse(message);
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
            string stringContent = @"";
            
            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
            message.Content = new StringContent(stringContent);
            return await NetworkResponse<SubmitEventResponse>.FromHttpResponse(message);
        }

        /// <summary>
        /// Gets a list of all of the GameEvents for the specified gameroom
        /// </summary>
        /// <param name="gameRoom">The id of the game room to fetch events for.</param>
        /// <returns>A list of game events</returns>
        public async Task<NetworkResponse<GameEventResponse>> GetGameEvents(int gameRoom)
        {
            string stringContent = @"[ { ""event_id"": 1, ""time_issued"": 1587326523, ""occurs_at"": 15, ""player_id"": 1, ""event_msg"": ""{'MyJsonKe':'test'}"" } ]";
            
            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
            message.Content = new StringContent(stringContent);
            return await NetworkResponse<GameEventResponse>.FromHttpResponse(message);
        }

        /////////////////////////////////////////////////////////////////
        //
        // Messaging Methods
        //
        /////////////////////////////////////////////////////////////////

        public async Task<NetworkResponse<GroupMessageListResponse>> GetGroupMessages(int gameRoom, int GroupNumber)
        {
            string stringContent = @"[ { ""created_at"": ""2020-04-19 20:27:14"", ""sender_player_id"": 1, ""message"": ""Hello how are you?"" }, { ""created_at"": ""2020-04-19 20:27:14"", ""sender_player_id"": 2, ""message"": ""Hello"" }, { ""created_at"": ""2020-04-19 20:27:14"", ""sender_player_id"": 3, ""message"": ""Hi"" }, { ""created_at"": ""2020-04-19 20:27:14"", ""sender_player_id"": 1, ""message"": ""Test"" }, { ""created_at"": ""2020-04-19 20:27:14"", ""sender_player_id"": 1, ""message"": ""Hi"" }, { ""created_at"": ""2020-04-19 20:27:14"", ""sender_player_id"": 2, ""message"": ""Hmmm"" } ]";
            
            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
            message.Content = new StringContent(stringContent);
            return await NetworkResponse<GroupMessageListResponse>.FromHttpResponse(message);
        }


        public async Task<NetworkResponse<CreateGroupResponse>> CreateGroup(int gameRoom, List<Player> players)
        {
            string stringContent = @"{ ""id"": 1, ""room_id"": 1 }";
            
            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
            message.Content = new StringContent(stringContent);
            return await NetworkResponse<CreateGroupResponse>.FromHttpResponse(message);
        }
        
        public async Task<NetworkResponse<SendMessageResponse>> SendMessage(int gameRoom, int groupId, string message)
        {
            string stringContent = @"";
            
            HttpResponseMessage request = new HttpResponseMessage(HttpStatusCode.OK);
            request.Content = new StringContent(stringContent);
            return await NetworkResponse<SendMessageResponse>.FromHttpResponse(request);
        }
        
        /////////////////////////////////////////////////////////////////
        //
        // Social Methods
        //
        /////////////////////////////////////////////////////////////////

        public async Task<NetworkResponse<BlockPlayerResponse>> BlockPlayer(Player player)
        {
            string stringContent = @"";
            
            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
            message.Content = new StringContent(stringContent);
            return await NetworkResponse<BlockPlayerResponse>.FromHttpResponse(message);
        }

        public async Task<NetworkResponse<UnblockPlayerResponse>> UnblockPlayer(int BlockId)
        {
            string stringContent = @"";
            
            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
            message.Content = new StringContent(stringContent);
            return await NetworkResponse<UnblockPlayerResponse>.FromHttpResponse(message);
        }

        public async Task<NetworkResponse<BlockPlayerResponse>> GetBlockList()
        {   
            string stringContent = @"[ { ""sender_player_id"": 1, ""recipient_player_id"": 2 } ]";
            
            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
            message.Content = new StringContent(stringContent);
            return await NetworkResponse<BlockPlayerResponse>.FromHttpResponse(message);
        }
    }
}