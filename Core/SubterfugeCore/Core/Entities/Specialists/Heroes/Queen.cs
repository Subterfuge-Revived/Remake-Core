using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities.Specialists.Heroes
{
    public class Queen: Specialist
    {
        private int shieldDelta = 25;
        
        public Queen(Player owner) : base(owner, true)
        {
        }

        public override void ArriveAtLocation(IEntity entity, TimeMachine timeMachine)
        {
            if (!this._isCaptured)
            {
                entity.GetComponent<ShieldManager>().AlterShieldCapacity(shieldDelta);
            }
        }

        public override void LeaveLocation(IEntity entity, TimeMachine timeMachine)
        {
            
            if (!this._isCaptured)
            {
                entity.GetComponent<ShieldManager>().AlterShieldCapacity(shieldDelta * -1);
            }
        }

        public override void OnCapture(bool isCaptured, IEntity entity, TimeMachine timeMachine)
        {
            // TODO: Check for princesses to take over.
            // Otherwise, kill player.
            entity.GetComponent<DrillerCarrier>().GetOwner().SetEliminated(true);
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Queen;
        }

        public override string GetDescription()
        {
            return $"Adds ${shieldDelta} shields to the Queen's Location. If the Queen dies, the owner is eliminated.";
        }
    }
}