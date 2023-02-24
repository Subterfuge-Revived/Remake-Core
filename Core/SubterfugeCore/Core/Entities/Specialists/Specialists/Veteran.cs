using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Components;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Veteran : Specialist
    {
        public Veteran(Player owner, Priority priority) : base(owner, priority)
        {
        }

        public override void ArriveAt(IEntity entity)
        {
            entity.GetComponent<PositionManager>().OnPreCombat += PreCombatModification;
        }

        public override void LeaveLocation(IEntity entity)
        {
            entity.GetComponent<PositionManager>().OnPreCombat -= PreCombatModification;
        }

        public override SpecialistIds GetSpecialistId()
        {
            return SpecialistIds.Veteran;
        }

        private void PreCombatModification(object sender, OnPreCombatEventArgs eventArgs)
        {
            
        }
    }
}