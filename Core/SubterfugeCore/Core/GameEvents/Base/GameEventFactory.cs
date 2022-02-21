using SubterfugeRemakeService;
using SubterfugeCore.Core.GameEvents.PlayerTriggeredEvents;

namespace SubterfugeCore.Core.GameEvents.Base
{
    public static class GameEventFactory
    {
        public static PlayerTriggeredEvent ParseGameEvent(GameEventModel model)
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