using System.Collections.Generic;
using SubterfugeCore.Core.Components;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.GameEvents.Validators;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core.GameEvents.NaturalGameEvents.combat
{
    /// <summary>
    /// CombatEvent. It is considered a 'combat' if you arrive at any outpost, even your own.
    /// </summary>
    public class CombatEvent : NaturalGameEvent
    {
        
        /// <summary>
        /// One of the two combat participants
        /// </summary>
        private readonly Entity _combatant1;
        
        /// <summary>
        /// One of the two combat participants
        /// </summary>
        private readonly Entity _combatant2;
        
        /// <summary>
        /// A list of combat actions that will occur when the event is triggered.
        /// </summary>
        private readonly List<IReversible> _actions = new List<IReversible>();

        /// <summary>
        /// Constructor for the combat event
        /// </summary>
        /// <param name="combatant1">The first combatant</param>
        /// <param name="combatant2">The second combatant</param>
        /// <param name="tick">The tick the combat occurs</param>
        public CombatEvent(Entity combatant1, Entity combatant2, GameTick tick) : base(tick, Priority.NaturalPriority9)
        {
            this._combatant1 = combatant1;
            this._combatant2 = combatant2;
        }

        public override bool ForwardAction(TimeMachine timeMachine, GameState.GameState state)
        {
            if (!Validator.ValidateICombatable(state, _combatant1) || !Validator.ValidateICombatable(state, _combatant2))
            {
                this.EventSuccess = false;
                return false;
            }
            
            // Determine additional events that should be triggered for this particular combat.
            if (_combatant1.GetComponent<DrillerCarrier>().GetOwner() == _combatant2.GetComponent<DrillerCarrier>().GetOwner())
            {
                this._actions.Add(new FriendlySubArrive(_combatant1, _combatant2, base.GetOccursAt()));
            } else
            {
                this._actions.Add(new SpecialistCombat(_combatant1, _combatant2));
                this._actions.Add(new DrillerCombat(_combatant1, _combatant2));
                this._actions.Add(new CombatCleanup(_combatant1, _combatant2));
            }

            foreach (IReversible action in this._actions)
            {
                action.ForwardAction(timeMachine, state);
            }
            this.EventSuccess = true;
            return true;
        }

        public override bool BackwardAction(TimeMachine timeMachine, GameState.GameState state)
        {
            if (EventSuccess)
            {
                // perform actions in reverse
                for (int i = _actions.Count - 1; i >= 0; i--)
                {
                    this._actions[i].BackwardAction(timeMachine, state);
                }
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

        /// <summary>
        /// Returns a list of two objects containing both objects participating in combat.
        /// </summary>
        /// <returns>A list of the combatants</returns>
        public List<Entity> GetCombatants()
        {
            List<Entity> combatants = new List<Entity>
            {
                _combatant1,
                _combatant2
            };
            return combatants;
        }
    }
}
