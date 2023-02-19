using System;
using Subterfuge.Remake.Core.Config;
using Subterfuge.Remake.Core.Players.Currency;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Components
{
    public class CurrencyProducer : ResourceProducer
    {
        private TimeMachine _timeMachine;
        
        public CurrencyProducer(
            TimeMachine timeMachine
        ) : base(
            (int)(1440 / GameTick.MinutesPerTick), // One per day
            1,
            timeMachine
        )
        {
            _timeMachine = timeMachine;
        }

        public override void Produce(int productionAmount)
        {
            _timeMachine
                .GetState()
                .GetPlayers()
                .ForEach(player =>
                {
                    player.CurrencyManager.AddCurrency(CurrencyType.Specialist, productionAmount);
                });
        }

        public override void UndoProduce(int amountToRevert)
        {
            _timeMachine
                .GetState()
                .GetPlayers()
                .ForEach(player =>
                {
                    player.CurrencyManager.AddCurrency(CurrencyType.Specialist, amountToRevert * -1);
                });
        }

        public override int GetNextProductionAmount(GameState.GameState state, int baseValuePerProduction)
        {
            return 1;
        }
    }
}