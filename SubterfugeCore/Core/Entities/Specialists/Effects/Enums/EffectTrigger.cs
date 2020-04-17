namespace SubterfugeCore.Core.Entities.Specialists.Effects
{
    public enum EffectTrigger
    {
        /// <summary>
        /// Trigger on specialist hires
        /// </summary>
        Hire,
        
        /// <summary>
        /// Trigger on specialist promotions
        /// </summary>
        Promote,
        
        /// <summary>
        /// Trigger on sub launches
        /// </summary>
        SubLaunch,
        
        /// <summary>
        /// Trigger on subs arriving
        /// </summary>
        SubArrive,
        
        /// <summary>
        /// Trigger on combat events
        /// </summary>
        Combat,
        
        /// <summary>
        /// Trigger on combat losses
        /// </summary>
        CombatLoss,
        
        /// <summary>
        /// Trigger on combat victories
        /// </summary>
        CombatVictory,
        
        /// <summary>
        /// Trigger on factory productions
        /// </summary>
        FactoryProduce,
        
        /// <summary>
        /// Trigger on neptunium productions
        /// </summary>
        MineProduce,
        
        /// <summary>
        /// Trigger when subs enter the defined range.
        /// WARNING: Can only be used if TriggerRange is set to have a range.
        /// This event will not trigger when TriggerRange is set to Self or Local!
        /// </summary>
        SubEnter,
    }
}
