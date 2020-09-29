using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.Players;

namespace SubterfugeCore.Core.Network
{
    public interface INetworkCaller
    {

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
        Task<NetworkResponse<RegisterResponse>> RegisterAccount(string username, string password, string email);
        
        /// <summary>
        /// Logs the user into the API. If successful, stores the user's session token
        /// in the Api object so that future requests don't need to accept the token.
        /// Note: creating a `new Api()` instance will have the user's token persist.
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <param name="password">The user's password</param>
        /// <returns>The login response</returns>
        Task<NetworkResponse<LoginResponse>> Login(string username, string password);
        
        /////////////////////////////////////////////////////////////////
        //
        // GameRoom Methods
        //
        /////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets a list of all open game rooms
        /// </summary>
        /// <returns>A list of open game rooms</returns>
        Task<NetworkResponse<GameRoomResponse>> GetOpenRooms();
        
        /// <summary>
        /// Gets a list of all ongoing rooms that the user is a member of
        /// </summary>
        /// <returns>A list of ongoing game rooms</returns>
        Task<NetworkResponse<GameRoomResponse>> GetOngoingRooms();

        /// <summary>
        /// Join a game room
        /// </summary>
        /// <param name="roomId">The room id of the game to join</param>
        /// <returns>The JoinLobbyResponse</returns>
        Task<NetworkResponse<JoinLobbyResponse>> JoinLobby(int roomId);
        
        /// <summary>
        /// Removes the player from the specified game lobby
        /// </summary>
        /// <param name="roomId">The room id to remove the player from</param>
        /// <returns>The LeaveLobbyResponse</returns>
        Task<NetworkResponse<LeaveLobbyResponse>> LeaveLobby(int roomId);
        
        /// <summary>
        /// Starts a game early if the room's capacity is not filled.
        /// This action can only be performed by the creator of the lobby.
        /// </summary>
        /// <param name="roomId">The room id to start early.</param>
        /// <returns>the StartLobbyEarlyResponse</returns>
        Task<NetworkResponse<StartLobbyEarlyResponse>> StartLobbyEarly(int roomId);
        
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
        Task<NetworkResponse<CreateLobbyResponse>> CreateLobby(string title, int maxPlayers, int minRating, bool rated, bool anonymous, string goal, int map);

        /// <summary>
        /// Submits a game event to the a game room. Note: The player must be in the game room
        /// to be able to submit an event to it and the GameEvent must be a valid game event.
        /// </summary>
        /// <param name="gameEvent">The GameEvent to submit to the server</param>
        /// <param name="gameRoom">The id of the game room to submit an event to.</param>
        /// <returns>The SubmitEventResponse</returns>
        Task<NetworkResponse<SubmitEventResponse>> SubmitGameEvent(GameEvent gameEvent, int gameRoom);

        /// <summary>
        /// Gets a list of all of the GameEvents for the specified gameroom
        /// </summary>
        /// <param name="gameRoom">The id of the game room to fetch events for.</param>
        /// <returns>A list of game events</returns>
        Task<NetworkResponse<GameEventResponse>> GetGameEvents(int gameRoom);

        /////////////////////////////////////////////////////////////////
        //
        // Messaging Methods
        //
        /////////////////////////////////////////////////////////////////

        Task<NetworkResponse<GroupMessageListResponse>> GetGroupMessages(int gameRoom, int GroupNumber);


        Task<NetworkResponse<CreateGroupResponse>> CreateGroup(int gameRoom, List<Player> players);
        
        Task<NetworkResponse<SendMessageResponse>> SendMessage(int gameRoom, int groupId, string message);
        
        /////////////////////////////////////////////////////////////////
        //
        // Social Methods
        //
        /////////////////////////////////////////////////////////////////

        Task<NetworkResponse<BlockPlayerResponse>> BlockPlayer(Player player);

        Task<NetworkResponse<UnblockPlayerResponse>> UnblockPlayer(int BlockId);

        Task<NetworkResponse<BlockPlayerResponse>> GetBlockList();
        
    }
}