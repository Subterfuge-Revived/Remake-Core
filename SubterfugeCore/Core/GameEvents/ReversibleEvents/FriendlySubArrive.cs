using System;
using System.Collections.Generic;
using System.Text;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Entities.Locations;
using SubterfugeCore.Core.Interfaces;

namespace SubterfugeCore.Core.GameEvents.ReversibleEvents
{
    /// <summary>
    /// Friendly sub arrival
    /// </summary>
    public class FriendlySubArrive : IReversible
    {
        Sub arrivingSub;
        Outpost outpost;

        /// <summary>
        /// Friendly sub arrival event
        /// </summary>
        /// <param name="combatant1">Combatant 1</param>
        /// <param name="combatant2">Combatant 2</param>
        public FriendlySubArrive(ICombatable combatant1, ICombatable combatant2)
        {
            this.arrivingSub = (Sub)(combatant1 is Sub ? combatant1 : combatant2);
            this.outpost = (Outpost)(combatant1 is Outpost ? combatant1 : combatant2);
        }

        /// <summary>
        /// Undoes the sub's arrival
        /// </summary>
        /// <returns>If the event was undone</returns>
        public bool backwardAction()
        {
            this.outpost.removeDrillers(this.arrivingSub.getDrillerCount());
            this.outpost.getSpecialistManager().removeSpecialists(this.arrivingSub.getSpecialistManager().getSpecialists());
            Game.timeMachine.getState().addSub(this.arrivingSub);
            return true;
        }

        /// <summary>
        /// Perfoms a friendly sub arrival
        /// </summary>
        /// <returns>If the event was successful</returns>
        public bool forwardAction()
        {
            this.outpost.addDrillers(this.arrivingSub.getDrillerCount());
            this.outpost.getSpecialistManager().addSpecialists(this.arrivingSub.getSpecialistManager().getSpecialists());
            Game.timeMachine.getState().removeSub(this.arrivingSub);
            return true;
        }
    }
}
