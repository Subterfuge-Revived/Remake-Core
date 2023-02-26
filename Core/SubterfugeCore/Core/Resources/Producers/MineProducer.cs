using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Resources.Producers
{
    public class MineProducer : ResourceProducer
    {
        private IEntity ProduceAt;
        public MineProducer(
            IEntity produceAt,
            TimeMachine timeMachine
        ) : base(
            (int)(1440 / GameTick.MinutesPerTick),
            1,
            timeMachine
        ) {
            ProduceAt = produceAt;
        }

        protected override void Produce(int productionAmount)
        {
            ProduceAt.GetComponent<DrillerCarrier>().GetOwner().AlterNeptunium(productionAmount);
        }

        protected override void UndoProduce(int amountToRevert)
        {
            ProduceAt.GetComponent<DrillerCarrier>().GetOwner().AlterNeptunium(amountToRevert);
        }

        public override int GetNextProductionAmount(GameState state)
        {
            var owner = ProduceAt.GetComponent<DrillerCarrier>().GetOwner();
            if (ProduceAt.GetComponent<DrillerCarrier>().IsDestroyed() || (owner != null && owner.IsEliminated()))
            {
                return 0;
            }
            return state.GetPlayerOutposts(ProduceAt.GetComponent<DrillerCarrier>().GetOwner()).Count * BaseValuePerProduction;
        }
    }
}