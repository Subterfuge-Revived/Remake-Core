using System.Collections.Generic;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.GameEvents.SpecialistEvents;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Advisor : Specialist
    {
        private List<int> capacityIncreaseDelta = new List<int>() { 1, 2, 2 };
        
        public Advisor(Player owner) : base(owner, false)
        {
        }

        public override void ArriveAtLocation(IEntity location, TimeMachine timeMachine)
        {
            if (!_isCaptured)
            {
                location.GetComponent<SpecialistManager>().AllowHireFromLocation();
                location.GetComponent<SpecialistManager>().AlterCapacity(capacityIncreaseDelta[_level]);
            }
        }

        public override void LeaveLocation(IEntity location, TimeMachine timeMachine)
        {
            // Don't care if captured, when leaving should always alter spec capacity at friendly locations.
            if (Equals(location.GetComponent<DrillerCarrier>().GetOwner(), _owner))
            {
                location.GetComponent<SpecialistManager>().DisallowHireFromLocation();
                location.GetComponent<SpecialistManager>().AlterCapacity(capacityIncreaseDelta[_level] * -1);
            }
        }

        public override void OnCapture(bool isCaptured, IEntity entity, TimeMachine timeMachine)
        {
            entity.GetComponent<SpecialistManager>().DisallowHireFromLocation();
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Advisor;
        }

        public override string GetDescription()
        {
            return $"Allows hiring specialists from the Advisor's location." +
                   $"Carry {capacityIncreaseDelta} additional specialists with the Advisor.";
        }
    }
}