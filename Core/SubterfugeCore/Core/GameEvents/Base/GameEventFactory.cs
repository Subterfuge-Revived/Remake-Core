using Google.Protobuf.WellKnownTypes;
using SubterfugeRemakeService;
using SubterfugeCore.Core.GameEvents.PlayerTriggeredEvents;

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
                case EventType.DrillMineEvent:
                    return new DrillMineEvent(model);
                case EventType.UnknownEvent:
                    return null;
                default:
                    return null;
            }
        }
    }
}