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
                var destinationOwner = GetEntitysDestinationOwner(entity);
                    
                if (Equals(destinationOwner, _owner))
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
            // Slow the sub down if we are on a sub travelling to an outpost we own.
            if (!_isCaptured && captureLocation is Sub)
            {
                var captureDestinationOwner = GetEntitysDestinationOwner(captureLocation);
                
                if (Equals(captureDestinationOwner, _owner))
                {
                    captureLocation.GetComponent<SpeedManager>().DecreaseSpeed(1.5f + (0.5f * _level));
                }
            }
        }

        private Player GetEntitysDestinationOwner(IEntity sub)
        {
            return sub
                .GetComponent<PositionManager>()
                .GetDestination()
                .GetComponent<DrillerCarrier>()
                .GetOwner();
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Smuggler;
        }
    }
}