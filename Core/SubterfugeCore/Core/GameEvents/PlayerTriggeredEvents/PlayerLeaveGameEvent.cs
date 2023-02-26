using System.Linq;
using Newtonsoft.Json;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.PlayerTriggeredEvents
{
    public class PlayerLeaveGameEvent : PlayerTriggeredEvent
    {
        public PlayerLeaveGameEvent(GameRoomEvent model) : base(model)
        {
        }
        
        public PlayerLeaveGameEventData GetEventData()
        {
            return JsonConvert.DeserializeObject<PlayerLeaveGameEventData>(Model.GameEventData.SerializedEventData);
        }


        public override bool ForwardAction(TimeMachine timeMachine, GameState state)
        {
            var playerToLeave = state.GetPlayers()
                .FirstOrDefault(player => player.GetId() == GetEventData().Player.Id);

            if (playerToLeave != null)
            {
                playerToLeave.SetEliminated(true);
                EventSuccess = true;
                return true;
            }
            EventSuccess = false;
            return false;
        }

        public override bool BackwardAction(TimeMachine timeMachine, GameState state)
        {
            var playerToLeave = state.GetPlayers()
                .FirstOrDefault(player => player.GetId() == GetEventData().Player.Id);

            if (playerToLeave != null)
            {
                playerToLeave.SetEliminated(false);
                EventSuccess = true;
                return true;
            }
            EventSuccess = false;
            return false;
        }

        public override bool WasEventSuccessful()
        {
            return EventSuccess;
        }
    }
}