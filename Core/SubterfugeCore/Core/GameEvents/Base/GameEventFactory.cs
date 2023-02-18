using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.GameEvents.PlayerTriggeredEvents;

namespace Subterfuge.Remake.Core.GameEvents.Base
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
                case EventDataType.GameEndEventData:
                    return new GameEndEvent(model);
                case EventDataType.PauseGameEventData:
                    return new PauseGameEvent(model);
                case EventDataType.UnpauseGameEventData:
                    return new UnpauseGameEvent(model);
                case EventDataType.PlayerLeaveGameEventData:
                    return new PlayerLeaveGameEvent(model);
                default:
                    return null;
            }
        }
    }
}