using System;
using Subterfuge.Remake.Core.Config;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
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
            Constants.TicksPerProduction,
            Constants.BaseFactoryProductionAmount,
            timeMachine
        ) {
            Parent = parent;
        }

        protected override void Produce(int productionAmount)
        {
            Parent.GetComponent<DrillerCarrier>().AlterDrillers(productionAmount);
        }

        protected override void UndoProduce(int amountToRevert)
        {
            Parent.GetComponent<DrillerCarrier>().AlterDrillers(amountToRevert * -1);
        }

        public override int GetNextProductionAmount(GameState.GameState state)
        {
            var owner = Parent.GetComponent<DrillerCarrier>().GetOwner();
            if (Parent.GetComponent<DrillerCarrier>().IsDestroyed() || (owner != null && owner.IsEliminated()))
            {
                return 0;
            }
            return Math.Min(state.GetExtraDrillerCapcity(Parent.GetComponent<DrillerCarrier>().GetOwner()), BaseValuePerProduction);
        }
    }
}