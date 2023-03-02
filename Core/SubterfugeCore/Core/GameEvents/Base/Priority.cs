namespace Subterfuge.Remake.Core.GameEvents.Base
{
    public enum Priority
    {
        PlayerIssuedCommand = 100,
        NaturalPriority9 = 99,
        
        RESOURCE_PRODUCTION = 90,
        
        // Combat priorities applied by specialists
        SPECIALIST_EXPLODE = 89,
        SPECIALIST_NEUTRALIZE_SPECIALIST_EFFECTS = 80,
        SPECIALIST_DEMOTE_EFFECT = 60,
        SPECIALIST_SHIELD_EFFECT = 40,
        SPECIALIST_DRILLER_EFFECT = 30,
        SPECIALIST_STEAL_EFFECT = 25,
        SPECIALIST_SUB_REDIRECT = 20,
        SPECIALIST_SLOW_EFFECT = 15,
        SPECIALIST_SWAP_SPECIALISTS_EFFECT = 12,
        SPECIALIST_KILL_SPECIALISTS = 10,
        
        // Natural Combat events
        SHIELD_COMBAT = 2,
        DRILLER_COMBAT = 1,
        
        // Ownership transfer and specialist capture happens here.
        // At this event, the combat is done.
        // Friendly combats immediately jump to this stage.
        COMBAT_RESOLVE = 0,
    }
}