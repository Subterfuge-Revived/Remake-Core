using System.Collections.Generic;
using Subterfuge.Remake.Core.Components;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.GameEvents.NaturalGameEvents.combat.CombatEvents;
using Subterfuge.Remake.Core.GameEvents.Validators;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.NaturalGameEvents.combat
{
    /// <summary>
    /// CombatEvent. It is considered a 'combat' if you arrive at any outpost, even your own.
    /// </summary>
    public class CombatEvent : NaturalGameEvent
    {
        
        /// <summary>
        /// One of the two combat participants
        /// </summary>
        private readonly IEntity _combatant1;
        
        /// <summary>
        /// One of the two combat participants
        /// </summary>
        private readonly IEntity _combatant2;

        public List<NaturalGameEvent> CombatEventList = new List<NaturalGameEvent>();

        /// <summary>
        /// Constructor for the combat event
        /// </summary>
        /// <param name="combatant1">The first combatant</param>
        /// <param name="combatant2">The second combatant</param>
        /// <param name="tick">The tick the combat occurs</param>
        public CombatEvent(IEntity combatant1, IEntity combatant2, GameTick tick) : base(tick, Priority.NaturalPriority9)
        {
            this._combatant1 = combatant1;
            this._combatant2 = combatant2;
            
            // Determine additional events that should be triggered for this particular combat.
            if (IsFriendlyCombat())
            {
                this.CombatEventList.Add(new FriendlySubArrive(_combatant1, _combatant2, base.GetOccursAt()));
            } else
            {
                // Note: Other combat effects will get added to the list of game events for this event through the PreCombat event listener!
                
                // Add the base driller and shield combat events.
                CombatEventList.Add(new NaturalDrillerCombatEffect());
                CombatEventList.Add(new NaturalShieldCombatEffect());
            }
        }

        public override bool ForwardAction(TimeMachine timeMachine, GameState.GameState state)
        {
            // Sort the combat event list by priority.
            
            CombatEventList.Sort();
            if (!Validator.ValidateICombatable(state, _combatant1) || !Validator.ValidateICombatable(state, _combatant2))
            {
                this.EventSuccess = false;
                return false;
            }

            // Sort the list in order of priority.
            // Default order is ascending so reverse it after.
            CombatEventList.Sort();
            CombatEventList.Reverse();
            CombatEventList.ForEach(action => action.ForwardAction(timeMachine, state));
            this.EventSuccess = true;
            return true;
        }

        private bool IsFriendlyCombat()
        {
            return _combatant1.GetComponent<DrillerCarrier>().GetOwner() ==
                   _combatant2.GetComponent<DrillerCarrier>().GetOwner();
        }

        public override bool BackwardAction(TimeMachine timeMachine, GameState.GameState state)
        {
            if (EventSuccess)
            {
                CombatEventList.Sort();
                CombatEventList.ForEach(action => action.BackwardAction(timeMachine, state));
            }

            return this.EventSuccess;
        }

        public override Priority GetPriority()
        {
            return Priority.NaturalPriority9;
        }

        public override bool WasEventSuccessful()
        {
            return this.EventSuccess;
        }

        public void AddEffectToCombat(NaturalGameEvent combatEffect)
        {
            CombatEventList.Add(combatEffect);
        }
    }
}
