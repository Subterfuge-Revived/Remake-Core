using NUnit.Framework;
using SubterfugeCore.Models.GameEvents;
using SubterfugeServerConsole.Connections;

namespace SubterfugeRestApiServerTest;

public class LobbyUtils
{
    public static async Task<CreateRoomResponse> CreateLobby(
        string roomName = "My room!",
        int maxPlayers = 5,
        bool isRanked = false,
        Goal goal = Goal.Domination
    ) {
            CreateRoomResponse roomResponse = await TestUtils.GetClient().LobbyClient.CreateNewRoom(CreateRoomRequest(roomName, maxPlayers: maxPlayers, isRanked: isRanked, goal: goal));
            Assert.AreEqual(roomResponse.Status.IsSuccess, true);
            Assert.IsTrue(roomResponse.GameConfiguration.Id != null);
            return roomResponse;
    }

    public static async Task<GetLobbyResponse> AssertPlayerInLobby(string playerId, bool isInLobby = true)
    {
        GetLobbyResponse lobbyResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest());
        Assert.AreEqual(lobbyResponse.Status.IsSuccess, true);
        Assert.AreEqual(isInLobby, lobbyResponse.Lobbies[0].PlayersInLobby.Any(it => it.Id == playerId));
        return lobbyResponse;
    }

    private static CreateRoomRequest CreateRoomRequest(
        string roomName,
        bool anon = false,
        bool isRanked = false,
        Goal goal = Goal.Domination,
        int maxPlayers = 5
    ) {
        return new CreateRoomRequest()
        {
            GameSettings = new GameSettings()
            {
                
                IsAnonymous = anon,
                Goal = goal,
                IsRanked = isRanked,
                MaxPlayers = maxPlayers,
                MinutesPerTick = 15,
                AllowedSpecialists = { }
            },
            IsPrivate = false,
            MapConfiguration = CreateMapConfiguration(),
            RoomName = roomName,
        };
    }
    
    private static MapConfiguration CreateMapConfiguration()
    {
        return new MapConfiguration()
        {
            DormantsPerPlayer = 5,
            MaximumOutpostDistance = 100,
            MinimumOutpostDistance = 5,
            OutpostDistribution = new OutpostDistribution()
            {
                FactoryWeight = 0.33f, 
                GeneratorWeight = 0.33f,
                WatchtowerWeight = 0.33f,
            },
            OutpostsPerPlayer = 3,
            Seed = 123123,
        };
    }
}