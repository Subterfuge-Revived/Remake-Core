using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Martyr : Specialist
    {
        public Martyr(Player owner) : base(owner, false)
        {
        }

        public override void ArriveAtLocation(IEntity entity, TimeMachine timeMachine)
        {
            if (_isCaptured)
            {
                entity.GetComponent<PositionManager>().OnRegisterCombatEffects += ExplodeOnCombatEffects;
            }
        }

        public override void LeaveLocation(IEntity entity, TimeMachine timeMachine)
        {
            if (_isCaptured)
            {
                entity.GetComponent<PositionManager>().OnRegisterCombatEffects -= ExplodeOnCombatEffects;
            }
        }

        public override void OnCapture(bool isCaptured, IEntity entity, TimeMachine timeMachine)
        {
            entity.GetComponent<PositionManager>().OnRegisterCombatEffects -= ExplodeOnCombatEffects;
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Martyr;
        }

        public override string GetDescription()
        {
            return $"When participating in combat, the Martyr explodes, destroying everything in it's wake.";
        }

        public void ExplodeOnCombatEffects(object? sender, OnRegisterCombatEventArgs registerCombat)
        {
            var friendlyEntity = registerCombat.CombatEvent.GetEntityOwnedBy(_owner);
            
            registerCombat.CombatEvent.AddEffectToCombat(new EntityExplodeEffect(
                registerCombat.CombatEvent.OccursAt,
                friendlyEntity,
                registerCombat.CurrentState
            ));
        }
    }
}