using System.Collections.Generic;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents;
using Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents.CombatResolve;
using Subterfuge.Remake.Core.GameEvents.Validators;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.Combat
{
    /// <summary>
    /// CombatEvent. It is considered a 'combat' if you arrive at any outpost, even your own.
    /// </summary>
    public class CombatEvent : NaturalGameEvent
    {
        public readonly IEntity _combatant1;
        public readonly IEntity _combatant2;

        public List<NaturalGameEvent> CombatEventList = new List<NaturalGameEvent>();
        private readonly CombatResolveEvent _combatResolveEvent;

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
            _combatResolveEvent = new CombatResolveEvent(tick, combatant1, combatant2, this);
        }

        public override bool ForwardAction(TimeMachine timeMachine, GameState state)
        {
            if (!Validator.ValidateICombatable(state, _combatant1) || !Validator.ValidateICombatable(state, _combatant2))
            {
                this.EventSuccess = false;
                return false;
            }

            // Sort the list in order of priority.
            CombatEventList.Sort();
            CombatEventList.ForEach(action =>
            {
                if (action is SpecialistNeutralizeEffect)
                {
                    // Don't do any of the other combat effects in the list.
                    return;
                }
                action.ForwardAction(timeMachine, state);
            });

            _combatResolveEvent.ForwardAction(timeMachine, state);
            
            this.EventSuccess = true;
            return true;
        }

        public bool IsFriendlyCombat()
        {
            return Equals(_combatant1.GetComponent<DrillerCarrier>().GetOwner(), _combatant2.GetComponent<DrillerCarrier>().GetOwner());
        }

        public override bool BackwardAction(TimeMachine timeMachine, GameState state)
        {
            if (EventSuccess)
            {
                _combatResolveEvent.BackwardAction(timeMachine, state);
                
                CombatEventList.Sort();
                CombatEventList.Reverse();
                CombatEventList.ForEach(action =>
                {
                    if (action is SpecialistNeutralizeEffect)
                    {
                        // Don't do any of the other combat effects in the list.
                        return;
                    }
                    action.BackwardAction(timeMachine, state);
                });
            }

            return this.EventSuccess;
        }

        public override bool WasEventSuccessful()
        {
            return this.EventSuccess;
        }

        public void AddEffectToCombat(NaturalGameEvent combatEffect)
        {
            CombatEventList.Add(combatEffect);
        }

        public IEntity? GetEntityOwnedBy(Player player)
        {
            if (Equals(_combatant1.GetComponent<DrillerCarrier>().GetOwner(), player))
            {
                return _combatant1;
            }
            if (Equals(_combatant2.GetComponent<DrillerCarrier>().GetOwner(), player))
            {
                return _combatant2;
            }

            return null;
        }
        
        public IEntity? GetEnemyEntity(Player player)
        {
            if (Equals(_combatant1.GetComponent<DrillerCarrier>().GetOwner(), player))
            {
                return _combatant2;
            }
            if (Equals(_combatant2.GetComponent<DrillerCarrier>().GetOwner(), player))
            {
                return _combatant1;
            }

            return null;
        }

        public CombatResolution GetCombatResolution()
        {
            return _combatResolveEvent.CombatResolution;
        }
    }
}
