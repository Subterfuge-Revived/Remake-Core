using System.Collections.Generic;
using System.Numerics;
using SubterfugeCore.Core.GameEvents;
using SubterfugeCore.Core.GameEvents.ReversibleEvents;
using SubterfugeCore.Core.GameEvents.Validators;
using SubterfugeCore.Core.Interfaces.Outpost;
using SubterfugeCore.Entities;
using SubterfugeCore.Timing;

namespace SubterfugeCore.GameEvents
{
    /// <summary>
    /// CombatEvent. It is considered a 'combat; if you arrive at any outpost, even your own.
    /// </summary>
    public class CombatEvent : GameEvent
    {
        private GameTick eventTick;    // the tick combat occurs
        private Vector2 combatLocation; // The combat location
        private ICombatable combatant1; // combat participant
        private ICombatable combatant2; // combat participany
        private List<IReversible> actions = new List<IReversible>();  // combat actions

        /// <summary>
        /// Constructor for the combat event
        /// </summary>
        /// <param name="combatant1">The first combatant</param>
        /// <param name="combatant2">The second combatant</param>
        /// <param name="tick">The tick the combat occurs</param>
        /// <param name="combatLocation">The location of the combat</param>
        public CombatEvent(ICombatable combatant1, ICombatable combatant2, GameTick tick, Vector2 combatLocation)
        {
            this.combatant1 = combatant1;
            this.combatant2 = combatant2;
            this.eventTick = tick;
            this.combatLocation = combatLocation;
            this.eventName = "Combat Event";
        }
        
        /// <summary>
        /// Performs the reverse operation of the event
        /// </summary>
        public override void eventBackwardAction()
        {
            if (eventSuccess)
            {
                // perform actions in reverse
                for (int i = actions.Count - 1; i >= 0; i--)
                {
                    this.actions[i].backwardAction();
                }
            }
        }

        /// <summary>
        /// Performs the forward operation of the event
        /// </summary>
        public override void eventForwardAction()
        {
            if (!Validator.validateICombatable(combatant1) || !Validator.validateICombatable(combatant2))
            {
                this.eventSuccess = false;
                return;
            }

            // Determine additional events that should be triggered for this particular combat.
            if (combatant1.getOwner() == combatant2.getOwner())
            {
                this.actions.Add(new FriendlySubArrive(combatant1, combatant2));
            } else
            {
                this.actions.Add(new SpecialistCombat(combatant1, combatant2));
                this.actions.Add(new DrillerCombat(combatant1, combatant2));

                if(combatant1 is Outpost || combatant2 is Outpost)
                {
                    this.actions.Add(new OwnershipTransfer(combatant1, combatant2));
                }

            }

            foreach (IReversible action in this.actions)
            {
                action.forwardAction();
            }
            this.eventSuccess = true;
        }
        
        /// <summary>
        /// Gets the tick the event occurs at
        /// </summary>
        /// <returns>The tick of the event</returns>
        public override GameTick getTick()
        {
            return this.eventTick;
        }
    }
}
