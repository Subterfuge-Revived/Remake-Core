using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Components
{
    public class MineProducer : ResourceProducer
    {
        private IEntity Parent;
        public MineProducer(
            IEntity parent,
            TimeMachine timeMachine
        ) : base(
            (int)(1440 / GameTick.MinutesPerTick),
            1,
            timeMachine
        ) {
            Parent = parent;
        }

        protected override void Produce(int productionAmount)
        {
            Parent.GetComponent<DrillerCarrier>().GetOwner().AlterNeptunium(productionAmount);
        }

        protected override void UndoProduce(int amountToRevert)
        {
            Parent.GetComponent<DrillerCarrier>().GetOwner().AlterNeptunium(amountToRevert);
        }

        public override int GetNextProductionAmount(GameState.GameState state)
        {
            var owner = Parent.GetComponent<DrillerCarrier>().GetOwner();
            if (Parent.GetComponent<DrillerCarrier>().IsDestroyed() || (owner != null && owner.IsEliminated()))
            {
                return 0;
            }
            return state.GetPlayerOutposts(Parent.GetComponent<DrillerCarrier>().GetOwner()).Count * BaseValuePerProduction;
        }
    }
}