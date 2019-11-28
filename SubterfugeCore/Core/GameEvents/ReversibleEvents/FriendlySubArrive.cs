using SubterfugeCore.Core.Interfaces.Outpost;
using SubterfugeCore.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeCore.Core.GameEvents.ReversibleEvents
{
    public class FriendlySubArrive : IReversible
    {
        Sub arrivingSub;
        Outpost outpost;

        public FriendlySubArrive(ICombatable combatant1, ICombatable combatant2)
        {
            this.arrivingSub = (Sub)(combatant1 is Sub ? combatant1 : combatant2);
            this.outpost = (Outpost)(combatant1 is Outpost ? combatant1 : combatant2);
        }

        public bool backwardAction()
        {
            this.outpost.removeDrillers(this.arrivingSub.getDrillerCount());
            this.outpost.getSpecialistManager().removeSpecialists(this.arrivingSub.getSpecialistManager().getSpecialists());
            GameServer.timeMachine.getState().addSub(this.arrivingSub);
            return true;
        }

        public bool forwardAction()
        {
            this.outpost.addDrillers(this.arrivingSub.getDrillerCount());
            this.outpost.getSpecialistManager().addSpecialists(this.arrivingSub.getSpecialistManager().getSpecialists());
            GameServer.timeMachine.getState().removeSub(this.arrivingSub);
            return true;
        }
    }
}
