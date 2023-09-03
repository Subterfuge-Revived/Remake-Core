using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities.Specialists.Heroes
{
    public class Queen: Specialist
    {
        public static int SHIELD_PROVIDED = 25;
        
        public Queen(Player owner) : base(owner, true)
        {
        }

        public override void ArriveAtLocation(IEntity entity, TimeMachine timeMachine)
        {
            if (!this._isCaptured)
            {
                entity.GetComponent<SpecialistManager>().AllowHireFromLocation();
                entity.GetComponent<ShieldManager>().AlterShieldCapacity(SHIELD_PROVIDED);
            }
        }

        public override void LeaveLocation(IEntity entity, TimeMachine timeMachine)
        {
            
            if (!this._isCaptured)
            {
                entity.GetComponent<SpecialistManager>().DisallowHireFromLocation();
                entity.GetComponent<ShieldManager>().AlterShieldCapacity(SHIELD_PROVIDED * -1);
            }
        }

        public override void OnCapture(bool isCaptured, IEntity entity, TimeMachine timeMachine)
        {
            // TODO: Check for princesses to take over.
            // Otherwise, kill player.
            entity.GetComponent<SpecialistManager>().DisallowHireFromLocation();
            entity.GetComponent<DrillerCarrier>().GetOwner().SetEliminated(true);
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Queen;
        }

        public override string GetDescription()
        {
            return $"Adds ${SHIELD_PROVIDED} shields to the Queen's Location. If the Queen dies, the owner is eliminated.";
        }
    }
}