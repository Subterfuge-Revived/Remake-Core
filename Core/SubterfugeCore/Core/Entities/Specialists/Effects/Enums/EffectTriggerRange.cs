namespace SubterfugeCore.Core.Entities.Specialists.Effects.Enums
{
    public enum EffectTriggerRange
    {
        /// <summary>
        /// Effect will only listen for events that happen to itself.
        /// This is similar to "Local", however, a trigger of "SubLaunch" set to "Local" will
        /// trigger on any sub getting launched from the specialist's location whereas a trigger of
        /// "SubLaunch" when set to "Self" will only listen for when this specialist gets launched.
        /// </summary>
        Self,
        
        /// <summary>
        /// Effect will trigger on all local effects no matter if the specialist is involved or not.
        /// The major difference here is that "SubLaunch" when set to local will detect all launches from
        /// the current location even if the specialist is not on board.
        /// </summary>
        Local,
        
        /// <summary>
        /// Effect is always applied in the same range.
        /// </summary>
        ConstantRange,
        
        /// <summary>
        /// Effect is applied to targets at the current location's vision.
        /// </summary>
        LocationVisionRange,
        
        /// <summary>
        /// Effect is triggered to targets within the player's vision
        /// </summary>
        PlayerVisionRange,
        
        /// <summary>
        /// Effect will trigger everywhere
        /// </summary>
        Global,
    }
}