namespace SubterfugeCore.Core.Entities.Specialists.Effects.Enums
{
    public enum EffectModifier
    {
        /// <summary>
        /// Add or remove drillers effect
        /// Note: To 'Steal' drillers, you place two specialist effects. First one AlterDriller to give friendly subs a scaled
        /// number of drillers based on the enemy's driller count. Second a AlterDriller to destroy the enemy's drillers.
        /// </summary>
        Driller,
        
        /// <summary>
        /// Alters the specialist capacity at this location.
        /// </summary>
        SpecialistCapacity,
        
        /// <summary>
        /// Alters the target's current shield value
        /// </summary>
        ShieldValue,
        
        /// <summary>
        /// Alters the shield regeneration rate
        /// </summary>
        ShieldRegeneration,
        
        /// <summary>
        /// Alters the maximum amount of shields
        /// </summary>
        ShieldMaxValue,
        
        /// <summary>
        /// Alters the target's current vision range
        /// </summary>
        Vision,
        
        /// <summary>
        /// Alters the speed
        /// </summary>
        Speed,
        
        /// <summary>
        /// Makes a player lose the game
        /// </summary>
        KillPlayer,
        
        /// <summary>
        /// Makes a player win the game
        /// </summary>
        VictoryPlayer,
        
        /// <summary>
        /// Destroys a specialist
        /// </summary>
        KillSpecialist,
        
        /// <summary>
        /// No effect.
        /// </summary>
        None,
    }
}