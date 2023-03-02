/*using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Helmsman : Specialist
    {
        public Helmsman(Player owner) : base(owner, false)
        {
        }

        public override void ArriveAt(IEntity entity)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<SpeedManager>().IncreaseSpeed(0.5f * _level);
            }
        }

        public override void LeaveLocation(IEntity entity)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<SpeedManager>().DecreaseSpeed(0.5f * _level);
            }
        }

        public override void OnCapturedEvent(IEntity captureLocation)
        {
            throw new System.NotImplementedException();
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Helmsman;
        }
    }
}*/