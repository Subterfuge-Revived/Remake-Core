using System;
using Subterfuge.Remake.Core.Config;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Components
{
    public class DrillerProducer : ResourceProducer
    {
        private IEntity Parent;
        public DrillerProducer(
            IEntity parent,
            TimeMachine timeMachine
        ) : base(
            (int)(Constants.MinutesPerProduction / GameTick.MinutesPerTick),
            Constants.BaseFactoryProduction,
            timeMachine
        ) {
            Parent = parent;
        }

        public override void Produce(int productionAmount)
        {
            Parent.GetComponent<DrillerCarrier>().AddDrillers(productionAmount);
        }

        public override void UndoProduce(int amountToRevert)
        {
            Parent.GetComponent<DrillerCarrier>().RemoveDrillers(amountToRevert);
        }

        public override int GetNextProductionAmount(GameState.GameState state, int baseValuePerProduction)
        {
            if (!Parent.GetComponent<DrillerCarrier>().IsDestroyed())
            {
                return 0;
            }
            return Math.Min(state.GetExtraDrillerCapcity(Parent.GetComponent<DrillerCarrier>().GetOwner()), baseValuePerProduction);
        }
    }
}