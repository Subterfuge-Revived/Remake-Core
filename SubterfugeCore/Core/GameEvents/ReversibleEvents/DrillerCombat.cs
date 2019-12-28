using SubterfugeCore.Core.GameEvents.Validators;
using SubterfugeCore.Core.Interfaces.Outpost;
using SubterfugeCore.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeCore.Core.GameEvents
{
    /// <summary>
    /// Driller combat action
    /// </summary>
    public class DrillerCombat : IReversible
    {
        ICombatable combatant1;
        ICombatable combatant2;
        bool eventSuccess = false;

        int preCombatDrillers1;
        int preCombatDrillers2;

        /// <summary>
        /// Driller combat constructor
        /// </summary>
        /// <param name="combatant1">Combatant 1</param>
        /// <param name="combatant2">Combatant 2</param>
        public DrillerCombat(ICombatable combatant1, ICombatable combatant2)
        {
            this.combatant1 = combatant1;
            this.combatant2 = combatant2;
        }

        /// <summary>
        /// Performs the reverse action of the driller combat to undo.
        /// </summary>
        /// <returns>if the event was reversed</returns>
        public bool backwardAction()
        {
            if (eventSuccess)
            {
                // Add any removed subs back.
                if (combatant1 is Sub && (combatant1.getDrillerCount() < 0 || (combatant1.getDrillerCount() == 0 && combatant1.getSpecialistManager().getSpecialistCount() == 0)))
                {
                    GameServer.timeMachine.getState().addSub((Sub)combatant1);
                }

                if (combatant2 is Sub && (combatant2.getDrillerCount() < 0 || (combatant2.getDrillerCount() == 0 && combatant2.getSpecialistManager().getSpecialistCount() == 0)))
                {
                    GameServer.timeMachine.getState().addSub((Sub)combatant2);
                }
                // Restore driller counts.
                combatant1.setDrillerCount(preCombatDrillers1);
                combatant2.setDrillerCount(preCombatDrillers2);
            }
            return eventSuccess;
        }

        /// <summary>
        /// Performs driller combat between two subs
        /// </summary>
        /// <returns>If the event was succesfull</returns>
        public bool forwardAction()
        {
            preCombatDrillers1 = combatant1.getDrillerCount();
            preCombatDrillers2 = combatant2.getDrillerCount();
            combatant1.removeDrillers(preCombatDrillers2);
            combatant2.removeDrillers(preCombatDrillers1);

            // Remove any subs that should be removed after combat.
            if (combatant1 is Sub && (combatant1.getDrillerCount() < 0 || (combatant1.getDrillerCount() == 0 && combatant1.getSpecialistManager().getSpecialistCount() == 0)))
            {
                GameServer.timeMachine.getState().removeSub((Sub)combatant1);
            }

            if (combatant2 is Sub && (combatant2.getDrillerCount() < 0 || (combatant2.getDrillerCount() == 0 && combatant2.getSpecialistManager().getSpecialistCount() == 0)))
            {
                GameServer.timeMachine.getState().removeSub((Sub)combatant2);
            }

            this.eventSuccess = true;
            return true;
        }
    }
}
