using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Escort : Specialist
    {
        public Escort(Player owner) : base(owner, false)
        {
        }

        public override void ArriveAt(IEntity entity)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<SpecialistManager>().AlterCapacity(1 * _level);
            }
        }

        public override void LeaveLocation(IEntity entity)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<SpecialistManager>().AlterCapacity(-1 * _level);
            }
        }

        public override void OnCaptured(IEntity captureLocation)
        {
            captureLocation.GetComponent<SpecialistManager>().AlterCapacity(-1 * _level);
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Escort;
        }
    }
}