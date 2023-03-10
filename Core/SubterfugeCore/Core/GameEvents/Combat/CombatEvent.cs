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
    public class CombatEvent : PositionalGameEvent
    {
        public readonly IEntity _combatant1;
        public readonly IEntity _combatant2;

        public List<PositionalGameEvent> CombatEventList = new List<PositionalGameEvent>();
        private readonly CombatResolveEvent _combatResolveEvent;

        /// <summary>
        /// Constructor for the combat event
        /// </summary>
        /// <param name="combatant1">The first combatant</param>
        /// <param name="combatant2">The second combatant</param>
        /// <param name="tick">The tick the combat occurs</param>
        public CombatEvent(IEntity combatant1, IEntity combatant2, GameTick tick) : base(tick, Priority.COMBAT_EVENT, combatant1)
        {
            this._combatant1 = combatant1;
            this._combatant2 = combatant2;
            _combatResolveEvent = new CombatResolveEvent(tick, combatant1, combatant2, this);
            CombatEventList.Add(_combatResolveEvent);
        }

        public override bool ForwardAction(TimeMachine timeMachine)
        {
            if (!Validator.ValidateICombatable(timeMachine.GetState(), _combatant1) || !Validator.ValidateICombatable(timeMachine.GetState(), _combatant2))
            {
                this.EventSuccess = false;
                return false;
            }

            var skipUnnaturalEvents = false;

            // Sort the list in order of priority.
            CombatEventList.Sort();
            CombatEventList.ForEach(action =>
            {
                if (action is NeutralizeSpecialistEffects)
                {
                    skipUnnaturalEvents = true;
                }

                if (skipUnnaturalEvents && IsEventUnnatural(action))
                {
                    // NoOp
                }
                else
                {
                    action.ForwardAction(timeMachine);
                }
            });

            this.EventSuccess = true;
            return true;
        }

        private bool IsEventUnnatural(GameEvent gameEvent)
        {
            return PriorityExtensions.NaturalEvents().Contains(gameEvent.Priority);
        }

        public bool IsFriendlyCombat()
        {
            return Equals(_combatant1.GetComponent<DrillerCarrier>().GetOwner(), _combatant2.GetComponent<DrillerCarrier>().GetOwner());
        }

        public override bool BackwardAction(TimeMachine timeMachine)
        {
            if (EventSuccess)
            {
                var skipUnnaturalEvents = false;
                
                CombatEventList.Sort();
                CombatEventList.Reverse();
                CombatEventList.ForEach(action =>
                {
                    if (skipUnnaturalEvents && IsEventUnnatural(action))
                    {
                        // NoOp
                    }
                    else
                    {
                        action.BackwardAction(timeMachine);
                    }
                    
                    if (action is NeutralizeSpecialistEffects)
                    {
                        skipUnnaturalEvents = true;
                    }
                });
            }

            return this.EventSuccess;
        }

        public override bool WasEventSuccessful()
        {
            return this.EventSuccess;
        }

        public void AddEffectToCombat(PositionalGameEvent combatEffect)
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
