using System;
using System.Collections.Generic;
using System.Linq;
using SubterfugeRemakeService;

namespace SubterfugeServerConsole.Connections
{
    public class RoomModelMapper : ProtoClassMapper<RoomModel>
    {

        public string Id;
        public RoomStatus RoomStatus;
        public string CreatorId;
        public RankedInformation RankedInformation;
        public bool Anonymous;
        public string RoomName;
        public Goal Goal;
        public int Seed;
        public long UnixTimeCreated;
        public long UnixTimeStarted;
        public long MaxPlayers;
        public List<String> AllowedSpecialists;
        public double MinutesPerTick;
        public List<String> PlayersInGame;
        
        
        /// <summary>
        /// Maps the Protobuf model to the internal class
        /// </summary>
        /// <param name="userModel"></param>
        public RoomModelMapper(RoomModel roomModel)
        {
            Id = roomModel.Id;
            RoomStatus = roomModel.RoomStatus;
            CreatorId = roomModel.CreatorId;
            RankedInformation = roomModel.RankedInformation;
            Anonymous = roomModel.Anonymous;
            RoomName = roomModel.RoomName;
            Goal = roomModel.Goal;
            Seed = roomModel.Seed;
            UnixTimeCreated = roomModel.UnixTimeCreated;
            UnixTimeStarted = roomModel.UnixTimeStarted;
            MaxPlayers = roomModel.MaxPlayers;
            AllowedSpecialists = roomModel.AllowedSpecialists.ToList();
            MinutesPerTick = roomModel.MinutesPerTick;
            PlayersInGame = roomModel.PlayersInGame.ToList();
        }

        public override RoomModel ToProto()
        {
            return new RoomModel()
            {
                Id = Id,
                RoomStatus = RoomStatus,
                CreatorId = CreatorId,
                RankedInformation = RankedInformation,
                Anonymous = Anonymous,
                Goal = Goal,
                Seed = Seed,
                UnixTimeCreated = UnixTimeCreated,
                UnixTimeStarted = UnixTimeStarted,
                MaxPlayers = MaxPlayers,
                AllowedSpecialists = { AllowedSpecialists },
                MinutesPerTick = MinutesPerTick,
                RoomName = RoomName,
                PlayersInGame = { PlayersInGame }
            };
        }
    }
}