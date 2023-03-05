using System;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents
{
    public class CombatShieldEffect : PositionalGameEvent
    {
        private readonly IEntity _combatant1;
        private readonly IEntity _combatant2;

        private int _preCombatShields1;
        private int _preCombatShields2;
        private int _preCombatDrillers1;
        private int _preCombatDrillers2;
        
        public CombatShieldEffect(
            GameTick occursAt,
            IEntity combatant1,
            IEntity combatant2
        ) : base(occursAt, Priority.SHIELD_COMBAT, combatant1)
        {
            _combatant1 = combatant1;
            _combatant2 = combatant2;
        }

        public override bool ForwardAction(TimeMachine timeMachine)
        {
            _preCombatDrillers1 = _combatant1.GetComponent<DrillerCarrier>().GetDrillerCount();
            _preCombatDrillers2 = _combatant2.GetComponent<DrillerCarrier>().GetDrillerCount();
            _preCombatShields1 = _combatant1.GetComponent<ShieldManager>().GetShields();
            _preCombatShields2 = _combatant2.GetComponent<ShieldManager>().GetShields();

            
            var shieldDamage1 = Math.Min(_preCombatShields1, _preCombatDrillers2);
            var shieldDamage2 = Math.Min(_preCombatShields2, _preCombatDrillers1);
            
            
            // Effect shields and driller counts
            _combatant2.GetComponent<DrillerCarrier>().AlterDrillers(shieldDamage1 * -1);
            _combatant1.GetComponent<ShieldManager>().AlterShields(shieldDamage1 * -1);
            
            _combatant1.GetComponent<DrillerCarrier>().AlterDrillers(shieldDamage2 * -1);
            _combatant2.GetComponent<ShieldManager>().AlterShields(shieldDamage2 * -1);
            return true;
        }

        public override bool BackwardAction(TimeMachine timeMachine)
        {
            _combatant1.GetComponent<DrillerCarrier>().SetDrillerCount(_preCombatDrillers1);
            _combatant2.GetComponent<DrillerCarrier>().SetDrillerCount(_preCombatDrillers2);
            _combatant1.GetComponent<ShieldManager>().SetShields(_preCombatShields1);
            _combatant2.GetComponent<ShieldManager>().SetShields(_preCombatShields2);
            return true;
        }

        public override bool WasEventSuccessful()
        {
            return true;
        }
    }
}