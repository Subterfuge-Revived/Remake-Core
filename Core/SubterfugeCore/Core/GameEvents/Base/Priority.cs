namespace SubterfugeCore.Core.GameEvents.Base
{
    public enum Priority
    {
        PLAYER_ISSUED_COMMAND = 100, // HIRE & PROMOTION commands
        NATURAL_PRIORITY_9 = 90, // OTHER PLAYER issued commands
        NATURAL_PRIORITY_8 = 80, // PRE-COMBAT specialist actions (e.g. sentry)
        NATURAL_PRIORITY_7 = 70, // FACTORY driller production
        NATURAL_PRIORITY_6 = 60, // GIFTS & FRIENDLY sub arrival
        NATURAL_PRIORITY_5 = 50, // SUB-SUB combat
        NATURAL_PRIORITY_4 = 40, // SUB-OUTPOST combat
        NATURAL_PRIORITY_3 = 30, // POST-COMBAT specialist actions (e.g. diplomat)
        NATURAL_PRIORITY_2 = 20,
        NATURAL_PRIORITY_1 = 10, // NEPTUNIUM mining
        LOW_PRIORTY = 0, // VISION events
    }
}