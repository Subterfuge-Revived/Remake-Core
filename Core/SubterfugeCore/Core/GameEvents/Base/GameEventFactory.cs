using SubterfugeCore.Core.GameEvents.PlayerTriggeredEvents;
using SubterfugeCore.Models.GameEvents;

namespace SubterfugeCore.Core.GameEvents.Base
{
    public static class GameEventFactory
    {
        public static PlayerTriggeredEvent ParseGameEvent(GameRoomEvent model)
        {
            if (model.GameEventData.EventData is LaunchEventData)
            {
                return new LaunchEvent(model);
            }
            if (model.GameEventData.EventData is ToggleShieldEventData)
            {
                return new ToggleShieldEvent(model);
            }
            if (model.GameEventData.EventData is DrillMineEventData)
            {
                return new DrillMineEvent(model);
            }

            return null;
        }
    }
}