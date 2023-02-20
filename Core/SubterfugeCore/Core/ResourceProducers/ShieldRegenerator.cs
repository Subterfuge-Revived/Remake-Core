using Subterfuge.Remake.Core.Config;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Components
{
    public class ShieldRegenerator : ResourceProducer
    {
        private IEntity Parent;
        public ShieldRegenerator(
            IEntity parent,
            TimeMachine timeMachine
        ) : base(
            Constants.BASE_SHIELD_REGENERATION_TICKS,
            1,
            timeMachine
        ) {
            Parent = parent;
        }

        protected override void Produce(int productionAmount)
        {
            Parent.GetComponent<ShieldManager>().AddShield(productionAmount);
        }

        protected override void UndoProduce(int amountToRevert)
        {
            Parent.GetComponent<ShieldManager>().RemoveShields(amountToRevert);
        }

        public override int GetNextProductionAmount(GameState.GameState state)
        {
            var owner = Parent.GetComponent<DrillerCarrier>().GetOwner();
            if (Parent.GetComponent<DrillerCarrier>().IsDestroyed() || (owner != null && owner.IsEliminated()))
            {
                return 0;
            }
            return BaseValuePerProduction;
        }
    }
}