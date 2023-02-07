using SubterfugeCore.Core.GameEvents.PlayerTriggeredEvents;
using SubterfugeCore.Models.GameEvents;

namespace SubterfugeCore.Core.GameEvents.Base
{
    public static class GameEventFactory
    {
        public static PlayerTriggeredEvent ParseGameEvent(GameRoomEvent model)
        {
            switch (model.GameEventData.EventDataType)
            {
                case EventDataType.LaunchEventData:
                    return new LaunchEvent(model);
                case EventDataType.DrillMineEventData:
                    return new DrillMineEvent(model);
                case EventDataType.ToggleShieldEventData:
                    return new ToggleShieldEvent(model);
                default:
                    return null;
            }
        }
    }
}