using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Bolster : Specialist
    {
        public Bolster(Player owner) : base(owner, false)
        {
        }

        public override void ArriveAt(IEntity entity)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<SpecialistManager>().GetSpecialists().ForEach(it => it.Promote());
            }
        }

        public override void LeaveLocation(IEntity entity)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<SpecialistManager>().GetSpecialists().ForEach(it => it.UndoPromote());
            }
        }

        public override void OnCaptured(IEntity captureLocation)
        {
            captureLocation.GetComponent<SpecialistManager>().GetSpecialists().ForEach(it => it.UndoPromote());
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Bolster;
        }
    }
}