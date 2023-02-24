using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Components;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Helmsman : Specialist
    {
        public Helmsman(Player owner, Priority priority) : base(owner, priority)
        {
        }

        public override void ArriveAt(IEntity entity)
        {
            entity.GetComponent<SpeedManager>().IncreaseSpeed(0.5f * _level);
        }

        public override void LeaveLocation(IEntity entity)
        {
            entity.GetComponent<SpeedManager>().DecreaseSpeed(0.5f * _level);
        }

        public override SpecialistIds GetSpecialistId()
        {
            return SpecialistIds.Helmsman;
        }
    }
}