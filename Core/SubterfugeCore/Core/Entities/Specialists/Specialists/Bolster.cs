using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Bolster : Specialist
    {
        public Bolster(Player owner) : base(owner, false)
        {
        }

        public override void ArriveAtLocation(IEntity entity, TimeMachine timeMachine)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<SpecialistManager>().GetSpecialists().ForEach(it => it.Promote());
            }
        }

        public override void LeaveLocation(IEntity entity, TimeMachine timeMachine)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<SpecialistManager>().GetSpecialists().ForEach(it => it.UndoPromote());
            }
        }

        public override void OnCapture(bool isCaptured, IEntity entity, TimeMachine timeMachine)
        {
            entity.GetComponent<SpecialistManager>().GetSpecialists().ForEach(it => it.UndoPromote());
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Bolster;
        }

        public override string GetDescription()
        {
            return $"All specialists at Bolster's location are promoted by 1 level.";
        }
    }
}