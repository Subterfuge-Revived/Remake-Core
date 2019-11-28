using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using SubterfugeCore.Core.Components.Outpost;
using SubterfugeCore.Core.Entities.Specialists;
using SubterfugeCore.Core.GameEvents;
using SubterfugeCore.Core.GameEvents.ReversibleEvents;
using SubterfugeCore.Core.GameEvents.Validators;
using SubterfugeCore.Core.Interfaces.Outpost;
using SubterfugeCore.Entities;
using SubterfugeCore.Timing;

namespace SubterfugeCore.GameEvents
{
    // It is considered a 'combat' if you are arriving at any outpost, even your own!
    public class CombatEvent : GameEvent
    {
        private GameTick eventTick;
        private Vector2 combatLocation;
        private ICombatable combatant1;
        private ICombatable combatant2;
        private List<IReversible> actions = new List<IReversible>();

        public CombatEvent(ICombatable combatant1, ICombatable combatant2, GameTick tick, Vector2 combatLocation)
        {
            this.combatant1 = combatant1;
            this.combatant2 = combatant2;
            this.eventTick = tick;
            this.combatLocation = combatLocation;
        }
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
        public override GameTick getTick()
        {
            return this.eventTick;
        }

        public Vector2 GetCombatLocation()
        {
            return this.combatLocation;
        }
    }
}
