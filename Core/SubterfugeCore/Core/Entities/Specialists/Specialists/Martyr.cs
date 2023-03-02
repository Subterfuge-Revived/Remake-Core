/*using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Martyr : Specialist
    {
        public Martyr(Player owner) : base(owner, false)
        {
        }

        public override void ArriveAt(IEntity entity)
        {
            if (_isCaptured)
            {
                entity.GetComponent<PositionManager>().OnPreCombat += ExplodeOnCombat;
            }
        }

        public override void LeaveLocation(IEntity entity)
        {
            if (_isCaptured)
            {
                entity.GetComponent<PositionManager>().OnPreCombat -= ExplodeOnCombat;
            }
        }

        public override void OnCapturedEvent(IEntity captureLocation)
        {
            captureLocation.GetComponent<PositionManager>().OnPreCombat -= ExplodeOnCombat;
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Martyr;
        }

        public void ExplodeOnCombat(object? sender, OnPreCombatEventArgs preCombat)
        {
            var friendlyEntity = preCombat.CombatEvent.GetEntityOwnedBy(_owner);
            
            preCombat.CombatEvent.AddEffectToCombat(new EntityExplodeEffect(
                preCombat.CombatEvent.GetOccursAt(),
                friendlyEntity,
                preCombat.CurrentState
            ));
        }
    }
}*/