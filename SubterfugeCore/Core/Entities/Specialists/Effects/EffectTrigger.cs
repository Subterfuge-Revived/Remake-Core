namespace SubterfugeCore.Core.Entities.Specialists.Effects
{
    public enum EffectTrigger
    {
        // Local event triggers allow specialists to listen for any events that occur at the outpost but do not involve the specialist itself.
        LocalSubLaunch,       // a sub launched from the outpost this specialist is at
        LocalSubArrive,       // a sub has arrived at the outpost this specialist is at
        LocalHire,             // a specialist was hired at the outpost this specialist is at

        // Self triggering events are to listen for any events that happen to the specialist itself.
        SelfHire,      // This specialist was hired
        SelfPromote,   // This specialist was promoted
        SelfLaunch,    // This specialist was launched
        SelfArrive,    // This specialist has arrived
        SelfCombat,    // This specialist is in combat
        SelfCombatLoss, // This specialist lost in combat
        SelfCombatVictory,    // This specialist has survived in combat
    }
}
