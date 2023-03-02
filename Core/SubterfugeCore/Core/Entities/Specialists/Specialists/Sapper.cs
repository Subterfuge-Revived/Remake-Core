/*using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Sapper : Specialist
    {
        public Sapper(Player owner) : base(owner, false)
        {
        }

        public override void ArriveAt(IEntity entity)
        {
            if (_isCaptured && !Equals(entity.GetComponent<DrillerCarrier>().GetOwner(), _owner))
            {
                entity.GetComponent<ShieldRegenerationComponent>().ShieldRegenerator.SetPaused(true);
            }
        }

        public override void LeaveLocation(IEntity entity)
        {
            if (_isCaptured)
            {
                entity.GetComponent<ShieldRegenerationComponent>().ShieldRegenerator.SetPaused(false);
            }
        }

        public override void OnCapturedEvent(IEntity captureLocation)
        {
            if (!Equals(captureLocation.GetComponent<DrillerCarrier>().GetOwner(), _owner))
            {
                captureLocation.GetComponent<ShieldRegenerationComponent>().ShieldRegenerator.SetPaused(true);
            }
        }

        public override void OnUncaptured(IEntity captureLocation)
        {
            captureLocation.GetComponent<ShieldRegenerationComponent>().ShieldRegenerator.SetPaused(false);
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Sapper;
        }
    }
}*/