using System.Collections.Generic;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Smuggler : Specialist
    {
        public List<float> SpeedPerLevel = new List<float>() { 0.5f, 1.0f, 1.5f };
        
        public Smuggler(Player owner) : base(owner, false)
        {
        }

        public override void ArriveAtLocation(IEntity entity, TimeMachine timeMachine)
        {
            if (!_isCaptured && entity is Sub)
            {
                var destinationOwner = GetOwnerOfSubDestination(entity);
                    
                if (Equals(destinationOwner, _owner))
                {
                    entity.GetComponent<SpeedManager>().IncreaseSpeed(SpeedPerLevel[_level]);
                }
            }
        }

        public override void LeaveLocation(IEntity entity, TimeMachine timeMachine)
        {
            // Do nothing.
        }

        public override void OnCapture(bool isCaptured, IEntity entity, TimeMachine timeMachine)
        {
            // Slow the sub down if we were captured while travelling to an outpost we own.
            if (!isCaptured && entity is Sub)
            {
                var captureDestinationOwner = GetOwnerOfSubDestination(entity);
                
                if (Equals(captureDestinationOwner, _owner))
                {
                    entity.GetComponent<SpeedManager>().DecreaseSpeed(SpeedPerLevel[_level]);
                }
            }
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Smuggler;
        }

        public override string GetDescription()
        {
            return $"When targeting a friendly outpost, increases speed by {SpeedPerLevel} units.";
        }


        private Player GetOwnerOfSubDestination(IEntity sub)
        {
            return sub
                .GetComponent<PositionManager>()
                .GetDestination()
                .GetComponent<DrillerCarrier>()
                .GetOwner();
        }
    }
}