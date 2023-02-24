using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Components;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Advisor : Specialist
    {
        public Advisor(Player owner) : base(owner, Priority.NaturalPriority9)
        {
        }

        public override void ArriveAt(IEntity entity)
        {
            entity.GetComponent<SpecialistManager>().AlterCapacity(1);
        }

        public override void LeaveLocation(IEntity entity)
        {
            entity.GetComponent<SpecialistManager>().AlterCapacity(-1);
        }

        public override SpecialistIds GetSpecialistId()
        {
            return SpecialistIds.Advisor;
        }
    }
}