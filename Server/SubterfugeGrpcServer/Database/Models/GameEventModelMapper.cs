using Google.Protobuf;
using MongoDB.Libmongocrypt;
using SubterfugeCore.Core;
using SubterfugeRemakeService;

namespace SubterfugeServerConsole.Connections
{
    public class GameEventModelMapper : ProtoClassMapper<GameEventModel>
    {
        public string Id;
        public long UnixTimeIssued;
        public int OccursAtTick;
        public string IssuedBy;
        public byte[] EventData;
        public EventType EventType;
        public string RoomId;

        public GameEventModelMapper(GameEventModel gameEventModel)
        {
            Id = gameEventModel.Id;
            UnixTimeIssued = gameEventModel.UnixTimeIssued;
            OccursAtTick = gameEventModel.OccursAtTick;
            IssuedBy = gameEventModel.IssuedBy;
            EventData = gameEventModel.EventData.ToByteArray();
            EventType = gameEventModel.EventType;
            RoomId = gameEventModel.RoomId;
        }
        
        
        public override GameEventModel ToProto()
        {
            return new GameEventModel()
            {
                Id = Id,
                UnixTimeIssued = UnixTimeIssued,
                OccursAtTick = OccursAtTick,
                IssuedBy = IssuedBy,
                EventData = ByteString.CopyFrom(EventData),
                EventType = EventType,
                RoomId = RoomId,
            };
        }
    }
}