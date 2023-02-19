using System;
using System.Collections.Generic;
using System.Linq;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Test
{
    public class TestUtils
    {
        public GameConfiguration GetDefaultGameConfiguration(List<Player> players)
        {
            GameConfiguration config = new GameConfiguration()
            {
                Id = "1",
                RoomStatus = RoomStatus.Open,
                Creator = new User() { Id = "1", Username = "1"},
                GameSettings = new GameSettings()
                {
                    IsAnonymous = false,
                    Goal = Goal.Domination,
                    MinutesPerTick = 15,
                    IsRanked = false,
                    MaxPlayers = players.Count
                },
                MapConfiguration = new MapConfiguration()
                {
                    Seed = 123123,
                    DormantsPerPlayer = 6,
                    MaximumOutpostDistance = 180,
                    MinimumOutpostDistance = 30,
                    OutpostsPerPlayer = 3,
                    OutpostDistribution = new OutpostDistribution()
                    {
                        FactoryWeight = 0.33f, 
                        GeneratorWeight = 0.33f,
                        WatchtowerWeight = 0.33f,
                    }
                },
                RoomName = "Room",
                TimeCreated = DateTime.UtcNow,
                TimeStarted = DateTime.MinValue,
                PlayersInLobby = players.Select(it => it.PlayerInstance.ToUser()).ToList(),
            };
            return config;
        }
    }
}