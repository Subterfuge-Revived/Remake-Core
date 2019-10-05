using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using SubterfugeCore.Entities;
using SubterfugeCore.Timing;

namespace SubterfugeCore.GameEvents
{
    class SubCombatEvent : GameEvent
    {
        private GameTick eventTick;
        private Vector2 combatLocation;
        private Sub sub1;
        private Sub sub2;
        private int sub1DrillersOnCombat;
        private int sub2DrillersOnCombat;

        public SubCombatEvent(Sub sub1, Sub sub2, GameTick tick, Vector2 combatLocation)
        {
            this.sub1 = sub1;
            this.sub2 = sub2;
            this.eventTick = tick;
            this.combatLocation = combatLocation;
        }
        public override void eventBackwardAction()
        {

            // Determine winner
            if (sub1DrillersOnCombat == sub2DrillersOnCombat)
            {
                GameServer.state.getSubList().Add(sub1);
                GameServer.state.getSubList().Add(sub2);
            }
            else if (sub1DrillersOnCombat > sub2DrillersOnCombat)
            {
                sub1.addDrillers(sub2DrillersOnCombat);
                GameServer.state.getSubList().Add(sub2);
            }
            else
            {
                sub2.addDrillers(sub1DrillersOnCombat);
                GameServer.state.getSubList().Add(sub1);

            }
        }

        public override void eventForwardAction()
        {
            sub1DrillersOnCombat = sub1.getDrillerCount();
            sub2DrillersOnCombat = sub2.getDrillerCount();

            // Determine winnter
            if(sub1DrillersOnCombat == sub2DrillersOnCombat)
            {
                GameServer.state.getSubList().Remove(sub1);
                GameServer.state.getSubList().Remove(sub2);
            } else if(sub1DrillersOnCombat > sub2DrillersOnCombat)
            {
                sub1.removeDrillers(sub2.getDrillerCount());
                GameServer.state.getSubList().Remove(sub2);
            } else
            {
                sub2.removeDrillers(sub1.getDrillerCount());
                GameServer.state.getSubList().Remove(sub1);

            }
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
