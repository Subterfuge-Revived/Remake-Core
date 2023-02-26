using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Veteran : Specialist
    {
        public Veteran(Player owner) : base(owner, false)
        {
        }

        public override void ArriveAt(IEntity entity)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<PositionManager>().OnPreCombat += PreCombatModification;
            }
        }

        public override void LeaveLocation(IEntity entity)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<PositionManager>().OnPreCombat -= PreCombatModification;
            }
        }

        public override void OnCaptured(IEntity captureLocation)
        {
            captureLocation.GetComponent<PositionManager>().OnPreCombat -= PreCombatModification;
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Veteran;
        }

        private void PreCombatModification(object sender, OnPreCombatEventArgs eventArgs)
        {
            var friendlyEntity = eventArgs.CombatEvent.GetEntityOwnedBy(_owner);
            
            eventArgs.CombatEvent.AddEffectToCombat(new SpecialistDrillerEffect(
                eventArgs.CombatEvent,
                friendlyEntity,
                0,
                -1 * GetEnemyDrillerDelta(friendlyEntity)
            ));
        }

        public int GetEnemyDrillerDelta(IEntity friendly)
        {
            if(_level == 2)
            {
                return 20;
            }

            if (_level == 3)
            {
                return 20 + (friendly.GetComponent<DrillerCarrier>().GetDrillerCount() / 5);
            }
            return 10;
        }
    }
}