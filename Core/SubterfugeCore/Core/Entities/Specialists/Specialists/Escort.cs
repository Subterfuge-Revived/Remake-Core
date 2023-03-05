using System.Collections.Generic;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Escort : Specialist
    {
        public List<int> deltaPerLevel = new List<int>() { 1, 2, 3 };
        
        public Escort(Player owner) : base(owner, false)
        {
        }

        public override void ArriveAtLocation(IEntity entity, TimeMachine timeMachine)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<SpecialistManager>().AlterCapacity(deltaPerLevel[_level]);
            }
        }

        public override void LeaveLocation(IEntity entity, TimeMachine timeMachine)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<SpecialistManager>().AlterCapacity(-1 * deltaPerLevel[_level]);
            }
        }

        public override void OnCapture(bool isCaptured, IEntity entity, TimeMachine timeMachine)
        {
            entity.GetComponent<SpecialistManager>().AlterCapacity(-1 * deltaPerLevel[_level]);
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Escort;
        }

        public override string GetDescription()
        {
            return $"The Escort can carry an additional {deltaPerLevel} specialists.";
        }
    }
}