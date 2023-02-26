using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Inspector : Specialist
    {
        public Inspector(Player owner) : base(owner, false)
        {
        }

        public override void ArriveAt(IEntity entity)
        {
            if (!_isCaptured)
            {
                var shieldManager = entity.GetComponent<ShieldManager>();
                shieldManager.SetShields(shieldManager.GetShieldCapacity());
                shieldManager.AlterShieldCapacity(10 * _level);
            }
        }

        public override void LeaveLocation(IEntity entity)
        {
            if (!_isCaptured)
            {
                var shieldManager = entity.GetComponent<ShieldManager>();
                shieldManager.AlterShieldCapacity(-10 * _level);
            }
        }

        public override void OnCaptured(IEntity captureLocation)
        {
            if (!_isCaptured)
            {
                var shieldManager = captureLocation.GetComponent<ShieldManager>();
                shieldManager.AlterShieldCapacity(-10 * _level);
            }
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Inspector;
        }
    }
}