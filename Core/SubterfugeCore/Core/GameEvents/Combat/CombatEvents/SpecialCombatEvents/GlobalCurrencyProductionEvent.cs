using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Players.Currency;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents
{
    public class GlobalCurrencyProductionEvent : GameEvent
    {
        public GlobalCurrencyProductionEvent(
            GameTick occursAt
        ) : base(occursAt, Priority.RESOURCE_PRODUCTION)
        {
        }

        public static void SpawnNewCurrencyEvent(TimeMachine timeMachine)
        {
            // Create another production event in the future.
            var ticksPerCurrencyGain = (int)(1440 / GameTick.MinutesPerTick);
            var newCurrencyEvent = new GlobalCurrencyProductionEvent(timeMachine.GetCurrentTick().Advance(ticksPerCurrencyGain));
            timeMachine.AddEvent(newCurrencyEvent);
        }

        public override bool ForwardAction(TimeMachine timeMachine)
        {
            timeMachine
                .GetState()
                .GetPlayers()
                .ForEach(player =>
                {
                    player.CurrencyManager.AddCurrency(CurrencyType.Specialist, 1);
                });
            
            // Create another production event in the future.
            SpawnNewCurrencyEvent(timeMachine);
            
            return true;
        }

        public override bool BackwardAction(TimeMachine timeMachine)
        {
            timeMachine
                .GetState()
                .GetPlayers()
                .ForEach(player =>
                {
                    player.CurrencyManager.AddCurrency(CurrencyType.Specialist, -1);
                });
            return true;
        }

        public override bool WasEventSuccessful()
        {
            return true;
        }

        public override string GetEventId()
        {
            return OccursAt.GetTick().ToString();
        }

        public override bool Equals(object other)
        {
            GlobalCurrencyProductionEvent asEvent = other as GlobalCurrencyProductionEvent;
            if (asEvent == null)
                return false;

            return asEvent.OccursAt == this.OccursAt;
        }

        public override int GetHashCode()
        {
            return OccursAt.GetHashCode();
        }
    }
}