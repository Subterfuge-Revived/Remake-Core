using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SubterfugeCore.Core.GameEvents;
using SubterfugeCore.Core.Network;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCoreTest
{
    [TestClass]
    public class ApiModelTest
    {

        public Api api; 

        [TestInitialize]
        public void setup()
        {
            api = new Api();
            api.developmentMode(true);
        }

        [TestMethod]
        public void testLoginSuccess()
        {
            Task.Run(async () =>
            {
                NetworkResponse<LoginResponse> response = await api.Login("username", "password");
            }).GetAwaiter().GetResult();
        }
        
        [TestMethod]
        public void testOpenRoomListSuccess()
        {
            Task.Run(async () =>
            {
                NetworkResponse<GameRoomResponse> response = await api.GetOpenRooms();
            }).GetAwaiter().GetResult();
        }
        
        [TestMethod]
        public void testOngoingRoomListSuccess()
        {
            Task.Run(async () =>
            {
                NetworkResponse<GameRoomResponse> response = await api.GetOngoingRooms();
            }).GetAwaiter().GetResult();
        }
        
        [TestMethod]
        public void testJoinLobby()
        {
            Task.Run(async () =>
            {
                NetworkResponse<JoinLobbyResponse> response = await api.JoinLobby(4);
            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void TestLeaveLobby()
        {
            Task.Run(async () =>
            {
                NetworkResponse<LeaveLobbyResponse> response = await api.LeaveLobby(4);
            }).GetAwaiter().GetResult();
        }
        
        [TestMethod]
        public void StartLobbyEarly()
        {
            Task.Run(async () =>
            {
                NetworkResponse<StartLobbyEarlyResponse> response = await api.StartLobbyEarly(3);
            }).GetAwaiter().GetResult();
        }
        
        [TestMethod]
        public void CreateLobby()
        {
            Task.Run(async () =>
            {
                NetworkResponse<CreateLobbyResponse> response = await api.CreateLobby("heyt", 4, 1250, false, false, "mine", 2);
            }).GetAwaiter().GetResult();
        }
        
        [TestMethod]
        public void SubmitGameEvent()
        {
            Task.Run(async () =>
            {
                NetworkResponse<SubmitEventResponse> response = await api.SubmitGameEvent(null, 4);
            }).GetAwaiter().GetResult();
        }
        
        [TestMethod]
        public void GetGameEvents()
        {
            Task.Run(async () =>
            {
                NetworkResponse<GameEventResponse> response = await api.GetGameEvents(4);
            }).GetAwaiter().GetResult();
        }
        
        [TestMethod]
        public void GetGroupMessages()
        {
            Task.Run(async () =>
            {
                NetworkResponse<GroupMessageListResponse> response = await api.GetGroupMessages(4, 4);
            }).GetAwaiter().GetResult();
        }
        
        [TestMethod]
        public void CreateGroup()
        {
            Task.Run(async () =>
            {
                NetworkResponse<CreateGroupResponse> response = await api.CreateGroup(4, new List<Player>());
            }).GetAwaiter().GetResult();
        }
        
        [TestMethod]
        public void SendMessage()
        {
            Task.Run(async () =>
            {
                NetworkResponse<SendMessageResponse> response = await api.SendMessage(4, 5, "hi");
            }).GetAwaiter().GetResult();
        }
        
        [TestMethod]
        public void BlockPlayer()
        {
            Task.Run(async () =>
            {
                NetworkResponse<BlockPlayerResponse> response = await api.BlockPlayer(null);
            }).GetAwaiter().GetResult();
        }
        
        [TestMethod]
        public void UnblockPlayer()
        {
            Task.Run(async () =>
            {
                NetworkResponse<UnblockPlayerResponse> response = await api.UnblockPlayer(6);
            }).GetAwaiter().GetResult();
        }
        
        [TestMethod]
        public void GetBlockList()
        {
            Task.Run(async () =>
            {
                NetworkResponse<BlockPlayerResponse> response = await api.GetBlockList();
            }).GetAwaiter().GetResult();
        }

    }
}