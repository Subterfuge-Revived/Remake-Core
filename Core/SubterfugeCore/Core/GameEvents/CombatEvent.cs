using System.Collections.Generic;
using System.Numerics;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.GameEvents.ReversibleEvents;
using SubterfugeCore.Core.GameEvents.Validators;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Core.Topologies;

namespace SubterfugeCore.Core.GameEvents
{
    /// <summary>
    /// CombatEvent. It is considered a 'combat' if you arrive at any outpost, even your own.
    /// </summary>
    public class CombatEvent : GameEvent
    {
        /// <summary>
        /// The tick the combat occurs on
        /// </summary>
        private GameTick _eventTick;
        
        /// <summary>
        /// Where the combat occurs
        /// </summary>
        private RftVector _combatLocation;
        
        /// <summary>
        /// One of the two combat participants
        /// </summary>
        private ICombatable _combatant1;
        
        /// <summary>
        /// One of the two combat participants
        /// </summary>
        private ICombatable _combatant2;
        
        /// <summary>
        /// A list of combat actions that will occur when the event is triggered.
        /// </summary>
        private List<IReversible> _actions = new List<IReversible>();

        /// <summary>
        /// Constructor for the combat event
        /// </summary>
        /// <param name="combatant1">The first combatant</param>
        /// <param name="combatant2">The second combatant</param>
        /// <param name="tick">The tick the combat occurs</param>
        /// <param name="combatLocation">The location of the combat</param>
        public CombatEvent(ICombatable combatant1, ICombatable combatant2, GameTick tick, RftVector combatLocation)
        {
            this._combatant1 = combatant1;
            this._combatant2 = combatant2;
            this._eventTick = tick;
            this._combatLocation = combatLocation;
            this.EventName = "Combat Event";
            
            // Determine additional events that should be triggered for this particular combat.
            if (_combatant1.GetOwner() == _combatant2.GetOwner())
            {
                this._actions.Add(new FriendlySubArrive(_combatant1, _combatant2));
            } else
            {
                this._actions.Add(new SpecialistCombat(_combatant1, _combatant2));
                this._actions.Add(new DrillerCombat(_combatant1, _combatant2));
                this._actions.Add(new CombatCleanup(_combatant1, _combatant2));
            }
        }
        
        /// <summary>
        /// Performs the reverse operation of the event
        /// </summary>
        public override bool BackwardAction()
        {
            if (EventSuccess)
            {
                // perform actions in reverse
                for (int i = _actions.Count - 1; i >= 0; i--)
                {
                    this._actions[i].BackwardAction();
                }
            }

            return this.EventSuccess;
        }

        public override bool WasEventSuccessful()
        {
            return this.EventSuccess;
        }

        public override string ToJson()
        {
            // Combat events shouldn't be stored in the database. No need for json.
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Performs the forward operation of the event
        /// </summary>
        public override bool ForwardAction()
        {
            if (!Validator.ValidateICombatable(_combatant1) || !Validator.ValidateICombatable(_combatant2))
            {
                this.EventSuccess = false;
                return false;
            }

            foreach (IReversible action in this._actions)
            {
                action.ForwardAction();
            }
            this.EventSuccess = true;
            return true;
        }
        
        /// <summary>
        /// Gets the tick the event occurs at
        /// </summary>
        /// <returns>The tick of the event</returns>
        public override GameTick GetTick()
        {
            return this._eventTick;
        }

        /// <summary>
        /// Returns a list of two objects containing both objects participating in combat.
        /// </summary>
        /// <returns>A list of the combatants</returns>
        public List<ICombatable> GetCombatants()
        {
            List<ICombatable> combatants = new List<ICombatable>();
            combatants.Add(_combatant1);
            combatants.Add(_combatant2);
            return combatants;
        }
    }
}
