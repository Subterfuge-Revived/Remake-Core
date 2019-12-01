using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public enum EffectTrigger
    {
        // Local event triggers allow specialists to listen for any events that occur at the outpost but do not involve the specialist itself.
        LOCAL_SUB_LAUNCH,       // a sub launched from the outpost this specialist is at
        LOCAL_SUB_ARRIVE,       // a sub has arrived at the outpost this specialist is at
        LOCAL_HIRE,             // a specialist was hired at the outpost this specialist is at

        // Self triggering events are to listen for any events that happen to the specialist itself.
        SELF_HIRE,      // This specialist was hired
        SELF_PROMOTE,   // This specialist was promoted
        SELF_LAUNCH,    // This specialist was launched
        SELF_ARRIVE,    // This specialist has arrived
        SELF_COMBAT,    // This specialist is in combat
        SELF_COMBAT_LOSS, // This specialist lost in combat
        SELF_COMBAT_VICTORY,    // This specialist has survived in combat
    }
}
