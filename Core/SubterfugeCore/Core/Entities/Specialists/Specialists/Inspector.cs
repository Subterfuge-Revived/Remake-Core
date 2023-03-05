using System.Collections.Generic;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Inspector : Specialist
    {
        private List<int> shieldsPerLevel = new List<int>() { 10, 20, 30 };
        
        public Inspector(Player owner) : base(owner, false)
        {
        }

        public override void ArriveAtLocation(IEntity entity, TimeMachine timeMachine)
        {
            if (!_isCaptured)
            {
                // TODO: This won't work. Need an undo action here.
                var shieldManager = entity.GetComponent<ShieldManager>();
                shieldManager.SetShields(shieldManager.GetShieldCapacity());
                shieldManager.AlterShieldCapacity(shieldsPerLevel[_level]);
            }
        }

        public override void LeaveLocation(IEntity entity, TimeMachine timeMachine)
        {
            
            if (!_isCaptured)
            {
                var shieldManager = entity.GetComponent<ShieldManager>();
                shieldManager.AlterShieldCapacity(shieldsPerLevel[_level] * -1);
            }
        }

        public override void OnCapture(bool isCaptured, IEntity entity, TimeMachine timeMachine)
        {
            if (!_isCaptured)
            {
                var shieldManager = entity.GetComponent<ShieldManager>();
                shieldManager.AlterShieldCapacity(shieldsPerLevel[_level] * -1);
            }
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Inspector;
        }

        public override string GetDescription()
        {
            return $"The Inspector increases the shield capacity at it's location by {shieldsPerLevel}." +
                   $"Regenerates all shields when arriving at friendly outposts.";
        }
    }
}