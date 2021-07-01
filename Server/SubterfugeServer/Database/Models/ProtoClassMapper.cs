using Google.Protobuf;

namespace SubterfugeServerConsole.Connections
{
    public abstract class ProtoClassMapper<T> where T : IMessage
    {
        public abstract T ToProto();
    }
}