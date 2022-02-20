using System;
using System.Collections.Generic;
using System.Linq;
using SubterfugeCore.Core.Players;
using SubterfugeRemakeService;
/*
namespace SubterfugeCoreTest
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
                    Anonymous = false,
                    Goal = Goal.Domination,
                    MinutesPerTick = 15,
                    IsRanked = false,
                    MaxPlayers = players.Count,
                    AllowedSpecialists = { }
                },
                MapConfiguration = new MapConfiguration()
                {
                    Seed = 123123,
                    DormantsPerPlayer = 6,
                    MaximumOutpostDistance = 180,
                    MinimumOutpostDistance = 30,
                    OutpostsPerPlayer = 3,
                    OutpostDistribution = new OutpostWeighting()
                    {
                        FactoryWeight = 0.33f, 
                        GeneratorWeight = 0.33f,
                        WatchtowerWeight = 0.33f,
                    }
                },
                RoomName = "Room",
                UnixTimeCreated = DateTime.UtcNow.Millisecond,
                UnixTimeStarted = 0,
            };
            config.Players.AddRange(players.Select(it => it.toUser()));
            return config;
        }
    }
}
*/