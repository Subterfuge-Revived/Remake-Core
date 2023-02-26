using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Smuggler : Specialist
    {
        public Smuggler(Player owner) : base(owner, false)
        {
        }

        public override void ArriveAt(IEntity entity)
        {
            if (!_isCaptured && entity is Sub)
            {
                if (Equals(
                        entity.GetComponent<PositionManager>().GetDestination().GetComponent<DrillerCarrier>()
                            .GetOwner(), _owner))
                {
                    entity.GetComponent<SpeedManager>().DecreaseSpeed(1.5f + (0.5f * _level));
                }
            }
        }

        public override void LeaveLocation(IEntity entity)
        {
            // Do nothing.
        }

        public override void OnCaptured(IEntity captureLocation)
        {
            // Do nothing.
            if (!_isCaptured && captureLocation is Sub)
            {
                if (Equals(
                        captureLocation.GetComponent<PositionManager>().GetDestination().GetComponent<DrillerCarrier>()
                            .GetOwner(), _owner))
                {
                    captureLocation.GetComponent<SpeedManager>().DecreaseSpeed(1.5f + (0.5f * _level));
                }
            }
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Smuggler;
        }
    }
}