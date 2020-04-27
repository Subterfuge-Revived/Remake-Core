namespace SubterfugeCore.Core.Entities.Specialists.Effects.Enums
{
    public enum EffectScale
    {
        /// <summary>
        /// Applies no scaling to the base value.
        /// The end result is a simple addition or subtraction of values.
        /// </summary>
        None,
        
        /// <summary>
        /// Scales the result by a constant value. If this type of scaling is used, the "BaseValue" of the effect
        /// is used as the scalar.
        /// </summary>
        ConstantValue,
        
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
        
        /// <summary>
        /// Scales the value based on the player's driller count.
        /// </summary>
        PlayerDrillerCount,
        
    }
}