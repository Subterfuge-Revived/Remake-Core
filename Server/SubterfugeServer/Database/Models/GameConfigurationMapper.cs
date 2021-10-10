using System;
using System.Collections.Generic;
using System.Linq;
using SubterfugeRemakeService;
using SubterfugeServerConsole.Connections.Models;

namespace SubterfugeServerConsole.Connections
{
    public class GameConfigurationMapper : ProtoClassMapper<GameConfiguration>
    {

        public string Id;
        public RoomStatus RoomStatus;
        public User Creator;
        
        /// <summary>
        ///  Game Settings Properties
        /// </summary>
        public List<SpecialistConfigurationMapper> SpecialistConfiguration;
        public double MinutesPerTick;
        public Goal Goal;
        public bool isRanked;
        public bool isAnonymous;
        public int MaxPlayers;

        /// <summary>
        /// Map Configuration Properties
        /// </summary>
        public int Seed;
        public int OutpostsPerPlayer;
        public int MinimumOutpostDistance;
        public int MaximumOutpostDistance;
        public int DormantsPerPlayer;
        public float GeneratorWeight;
        public float FactoryWeight;
        public float WatchtowerWeight;
        
        public string RoomName;
        public long UnixTimeCreated;
        public long UnixTimeStarted;
        public List<User> PlayersInGame;
        
        
        /// <summary>
        /// Maps the Protobuf model to the internal class
        /// </summary>
        /// <param name="userModel"></param>
        public GameConfigurationMapper(GameConfiguration gameConfiguration)
        {
            Id = gameConfiguration.Id;
            RoomStatus = gameConfiguration.RoomStatus;
            Creator = gameConfiguration.Creator;

            SpecialistConfiguration = gameConfiguration.GameSettings.AllowedSpecialists.Select(it => new SpecialistConfigurationMapper(it)).ToList();
            MinutesPerTick = gameConfiguration.GameSettings.MinutesPerTick;
            Goal = gameConfiguration.GameSettings.Goal;
            isRanked = gameConfiguration.GameSettings.IsRanked;
            isAnonymous = gameConfiguration.GameSettings.Anonymous;
            MaxPlayers = gameConfiguration.GameSettings.MaxPlayers;

            Seed = gameConfiguration?.MapConfiguration?.Seed ?? new Random().Next();
            OutpostsPerPlayer = gameConfiguration.MapConfiguration.OutpostsPerPlayer;
            MinimumOutpostDistance = gameConfiguration.MapConfiguration.MinimumOutpostDistance;
            MaximumOutpostDistance = gameConfiguration.MapConfiguration.MaximumOutpostDistance;
            DormantsPerPlayer = gameConfiguration.MapConfiguration.DormantsPerPlayer;
            GeneratorWeight = gameConfiguration.MapConfiguration.OutpostDistribution.GeneratorWeight;
            FactoryWeight = gameConfiguration.MapConfiguration.OutpostDistribution.FactoryWeight;
            WatchtowerWeight = gameConfiguration.MapConfiguration.OutpostDistribution.WatchtowerWeight;
            
            RoomName = gameConfiguration.RoomName;
            UnixTimeCreated = gameConfiguration.UnixTimeCreated;
            UnixTimeStarted = gameConfiguration.UnixTimeStarted;
            PlayersInGame = gameConfiguration.Players.ToList();
        }

        public override GameConfiguration ToProto()
        {
            return new GameConfiguration()
            {
                Id = Id,
                RoomStatus = RoomStatus,
                Creator = Creator,
                GameSettings = new GameSettings()
                {
                    Anonymous = isAnonymous,
                    Goal = Goal,
                    IsRanked = isRanked,
                    MaxPlayers = MaxPlayers,
                    MinutesPerTick = MinutesPerTick,
                },
                MapConfiguration = new MapConfiguration()
                {
                    DormantsPerPlayer = DormantsPerPlayer,
                    MaximumOutpostDistance = MaximumOutpostDistance,
                    MinimumOutpostDistance = MinimumOutpostDistance,
                    OutpostDistribution = new OutpostWeighting()
                    {
                        FactoryWeight = FactoryWeight,
                        GeneratorWeight = GeneratorWeight,
                        WatchtowerWeight = WatchtowerWeight,
                    },
                    OutpostsPerPlayer = OutpostsPerPlayer,
                    Seed = Seed
                },
                UnixTimeCreated = UnixTimeCreated,
                UnixTimeStarted = UnixTimeStarted,
                RoomName = RoomName,
                Players = { PlayersInGame }
            };
        }
    }
}