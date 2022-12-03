using SubterfugeCore.Core.GameEvents.PlayerTriggeredEvents;
using SubterfugeCore.Models.GameEvents;

namespace SubterfugeCore.Core.GameEvents.Base
{
    public static class GameEventFactory
    {
        public static PlayerTriggeredEvent ParseGameEvent(GameEventData model)
        {
            if (model.EventData is LaunchEventData)
            {
                return new LaunchEvent(model);
            }
            if (model.EventData is ToggleShieldEventData)
            {
                return new ToggleShieldEvent(model);
            }
            if (model.EventData is DrillMineEventData)
            {
                return new DrillMineEvent(model);
            }

            return null;
        }
    }
}