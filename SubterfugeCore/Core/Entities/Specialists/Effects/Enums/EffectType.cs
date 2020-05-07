namespace SubterfugeCore.Core.Entities.Specialists.Effects.Enums
{
    public enum EffectType
    {
        /// <summary>
        /// Add or remove drillers effect
        /// Note: To 'Steal' drillers, you place two specialist effects. First one AlterDriller to give friendly subs a scaled
        /// number of drillers based on the enemy's driller count. Second a AlterDriller to destroy the enemy's drillers.
        /// </summary>
        AlterDriller,
        
        /// <summary>
        /// Alters the maximum amount of shields
        /// </summary>
        AlterMaximumShield,
        
        /// <summary>
        /// Alters the shield regeneration rate
        /// </summary>
        AlterShieldRecharge,
        
        /// <summary>
        /// Alters the target's current shield value
        /// </summary>
        AlterCurrentShield,
        
        /// <summary>
        /// Alters the target's current vision range
        /// </summary>
        AlterVisionRange,
        
        /// <summary>
        /// Alters the specialist capacity at this location.
        /// </summary>
        AlterSpecialistCapacity,
        
        /// <summary>
        /// Alters the speed
        /// </summary>
        AlterSpeed,
        
        /// <summary>
        /// No effect.
        /// </summary>
        None,
    }
}