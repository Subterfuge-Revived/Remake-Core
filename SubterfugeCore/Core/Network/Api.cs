using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.Players;

namespace SubterfugeCore.Core.Network
{
    
    /// <summary>
    /// API class to communicate with the backend API
    /// </summary>
    public class Api : INetworkCaller
    {
        private INetworkCaller networkCaller { get; set; }
        
        public Api()
        {
            this.networkCaller = new NetworkCaller();
        }

        public Api(string url)
        {
            this.networkCaller = new NetworkCaller(url);
        }

        /// <summary>
        /// ONLY USE DURING DEVELOPMENT.
        /// Calling this method converts the network caller to a fake network caller
        /// that will return predfined JSON responses. This allows developers to bypass networking calls.
        /// NOTE: Calling these endpoints will NOT update the state of the netowkr.
        /// For example, a "join lobby" responds will not actually join the player to that lobby!!
        /// THIS CANNOT BE UNDONE.
        /// </summary>
        /// <param name="devMode">Set true to bypass the network interface.</param>
        public void developmentMode(bool devMode)
        {
            if (devMode)
            {
                this.networkCaller = new OfflineNetworkCaller();
            }
        }


        public Task<NetworkResponse<RegisterResponse>> RegisterAccount(string username, string password, string email)
        {
            return networkCaller.RegisterAccount(username, password, email);
        }

        public Task<NetworkResponse<LoginResponse>> Login(string username, string password)
        {
            return networkCaller.Login(username, password);
        }

        public Task<NetworkResponse<GameRoomResponse>> GetOpenRooms()
        {
            return networkCaller.GetOpenRooms();
        }

        public Task<NetworkResponse<GameRoomResponse>> GetOngoingRooms()
        {
            return networkCaller.GetOngoingRooms();
        }

        public Task<NetworkResponse<JoinLobbyResponse>> JoinLobby(int roomId)
        {
            return networkCaller.JoinLobby(roomId);
        }

        public Task<NetworkResponse<LeaveLobbyResponse>> LeaveLobby(int roomId)
        {
            return networkCaller.LeaveLobby(roomId);
        }

        public Task<NetworkResponse<StartLobbyEarlyResponse>> StartLobbyEarly(int roomId)
        {
            return networkCaller.StartLobbyEarly(roomId);
        }

        public Task<NetworkResponse<CreateLobbyResponse>> CreateLobby(string title, int maxPlayers, int minRating, bool rated, bool anonymous, string goal, int map)
        {
            return networkCaller.CreateLobby(title, maxPlayers, minRating, rated, anonymous, goal, map);
        }

        public Task<NetworkResponse<SubmitEventResponse>> SubmitGameEvent(GameEvent gameEvent, int gameRoom)
        {
            return networkCaller.SubmitGameEvent(gameEvent, gameRoom);
        }

        public Task<NetworkResponse<GameEventResponse>> GetGameEvents(int gameRoom)
        {
            return networkCaller.GetGameEvents(gameRoom);
        }

        public Task<NetworkResponse<GroupMessageListResponse>> GetGroupMessages(int gameRoom, int GroupNumber)
        {
            return networkCaller.GetGroupMessages(gameRoom, GroupNumber);
        }

        public Task<NetworkResponse<CreateGroupResponse>> CreateGroup(int gameRoom, List<Player> players)
        {
            return networkCaller.CreateGroup(gameRoom, players);
        }

        public Task<NetworkResponse<SendMessageResponse>> SendMessage(int gameRoom, int groupId, string message)
        {
            return networkCaller.SendMessage(gameRoom, groupId, message);
        }

        public Task<NetworkResponse<BlockPlayerResponse>> BlockPlayer(Player player)
        {
            return networkCaller.BlockPlayer(player);
        }

        public Task<NetworkResponse<UnblockPlayerResponse>> UnblockPlayer(int BlockId)
        {
            return networkCaller.UnblockPlayer(BlockId);
        }

        public Task<NetworkResponse<BlockPlayerResponse>> GetBlockList()
        {
            return networkCaller.GetBlockList();
        }
    }
}