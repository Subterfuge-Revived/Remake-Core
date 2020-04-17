namespace SubterfugeCore.Core.Entities.Specialists.Effects.Enums
{
    public enum EffectScale
    {
        /// <summary>
        /// Applies no scaling to the base value
        /// </summary>
        None,
        
        /// <summary>
        /// Scales the value by the player's total outpost count
        /// </summary>
        PlayerOutpostCount,
        
        /// <summary>
        /// Scales the value by the player's total mine count
        /// </summary>
        PlayerMineCount,
        
        /// <summary>
        /// Scales the value by the player's factory count
        /// </summary>
        PlayerFactoryCount,
        
    }
}