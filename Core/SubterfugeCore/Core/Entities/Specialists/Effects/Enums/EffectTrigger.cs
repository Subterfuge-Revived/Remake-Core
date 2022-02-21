namespace SubterfugeCore.Core.Entities.Specialists.Effects.Enums
{
    public enum EffectTrigger
    {
        /// <summary>
        /// No trigger.
        /// </summary>
        None,
        
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
        /// Trigger on all combat events
        /// </summary>
        Combat,
        
        /// <summary>
        /// Triggers only on sub-to-sub combat events.
        /// </summary>
        SubCombat,
        
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