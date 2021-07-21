using SubterfugeCore.Core.GameEvents.ReversibleEvents;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core.GameEvents.Reversible
{
    public class AlterDrillerEffect : IReversible
    {
        private IDrillerCarrier _drillerCarrier;
        private int _modifyBy;
        
        public AlterDrillerEffect(IDrillerCarrier drillerCarrier, int modifyBy)
        {
            _drillerCarrier = drillerCarrier;
            _modifyBy = modifyBy;
        }

        public void ForwardAction(TimeMachine timeMachine)
        {
            _drillerCarrier.AddDrillers(_modifyBy);
        }

        public void BackwardAction(TimeMachine timeMachine)
        {
            _drillerCarrier.RemoveDrillers(_modifyBy);
        }
    }
}