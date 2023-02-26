using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Resources.Producers
{
    public class ShieldRegenerator : ResourceProducer
    {
        private IEntity ProduceAt;
        public ShieldRegenerator(
            IEntity produceAt,
            TimeMachine timeMachine
        ) : base(
            Constants.BASE_SHIELD_REGENERATION_TICKS,
            1,
            timeMachine
        ) {
            ProduceAt = produceAt;
        }

        protected override void Produce(int productionAmount)
        {
            ProduceAt.GetComponent<ShieldManager>().AddShield(productionAmount);
        }

        protected override void UndoProduce(int amountToRevert)
        {
            ProduceAt.GetComponent<ShieldManager>().RemoveShields(amountToRevert);
        }

        public override int GetNextProductionAmount(GameState state)
        {
            var owner = ProduceAt.GetComponent<DrillerCarrier>().GetOwner();
            if (ProduceAt.GetComponent<DrillerCarrier>().IsDestroyed() || (owner != null && owner.IsEliminated()))
            {
                return 0;
            }
            return BaseValuePerProduction;
        }
    }
}