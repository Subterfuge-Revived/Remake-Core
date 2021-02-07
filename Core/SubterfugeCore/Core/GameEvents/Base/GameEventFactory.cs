using Google.Protobuf.WellKnownTypes;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.GameEvents.Base
{
    public class GameEventFactory
    {
        public static PlayerTriggeredEvent parseGameEvent(GameEventModel model)
        {
            switch (model.EventType)
            {
                case EventType.LaunchEvent:
                    return new LaunchEvent(model);
                case EventType.ToggleShieldEvent:
                    return new ToggleShieldEvent(model);
                case EventType.UnknownEvent:
                    return null;
                default:
                    return null;
            }
        }
    }
}