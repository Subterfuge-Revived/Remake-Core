using NUnit.Framework;
using Subterfuge.Remake.Api.Network;

namespace Subterfuge.Remake.Server.Test.util.account;

public class LobbyUtils
{
    public static async Task<CreateRoomResponse> CreateLobby(
        string roomName = "My room!",
        int maxPlayers = 5,
        bool isRanked = false,
        Goal goal = Goal.Domination
    )
    {
        var roomResponse = await TestUtils.GetClient().LobbyClient.CreateNewRoom(CreateRoomRequest(roomName, maxPlayers: maxPlayers, isRanked: isRanked, goal: goal));
        CreateRoomResponse room = roomResponse.GetOrThrow();
        Assert.AreEqual(roomResponse.IsSuccess(), true);
        Assert.IsTrue(room.GameConfiguration.Id != null);
        return room;
    }

    public static async Task<GetLobbyResponse> AssertPlayerInLobby(string playerId, bool isInLobby = true)
    {
        var roomResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest());
        GetLobbyResponse lobbyResponse = roomResponse.GetOrThrow();
        Assert.AreEqual(roomResponse.IsSuccess(), true);
        if (isInLobby)
        {
            Assert.AreEqual(isInLobby, lobbyResponse.Lobbies[0].PlayersInLobby.Any(it => it.Id == playerId));
        }
        else
        {
            if (lobbyResponse.Lobbies.Length > 0)
            {
                Assert.IsFalse(lobbyResponse.Lobbies[0].PlayersInLobby.Any(it => it.Id == playerId));
            }
        }

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
            },
            IsPrivate = false,
            MapConfiguration = CreateMapConfiguration(),
            RoomName = roomName,
            CreatorSpecialistDeck = new List<SpecialistIds>()
            {
                SpecialistIds.Advisor,
                SpecialistIds.Amnesiac,
                SpecialistIds.Assasin,
                SpecialistIds.Automation,
                SpecialistIds.Dispatcher,
                SpecialistIds.Economist,
                SpecialistIds.Enforcer,
                SpecialistIds.Engineer,
                SpecialistIds.Escort,
                SpecialistIds.Foreman,
                SpecialistIds.Helmsman,
                SpecialistIds.Hypnotist,
                SpecialistIds.Icicle,
                SpecialistIds.Industrialist,
                SpecialistIds.Infiltrator
            }
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