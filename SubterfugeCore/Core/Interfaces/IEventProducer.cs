using System;
using SubterfugeCore.Core.Entities.Specialists.Effects;
using SubterfugeCore.Core.Entities.Specialists.Effects.Enums;

namespace SubterfugeCore.Core.Interfaces
{
    public interface IEventProducer
    {
        /// <summary>
        /// Allows a listener to register itself. Listeners will get invoked when the event occurs.
        /// </summary>
        /// <param name="trigger">The event type that cause the trigger</param>
        /// <param name="friendly">The friendly unit (if any)</param>
        /// <param name="enemy">The enemy unit (if any)</param>
        void registerListener(EffectTrigger trigger, ICombatable friendly, ICombatable enemy);
        
        /// <summary>
        /// Triggers the forward event effect to any listeners.
        /// </summary>
        /// <param name="trigger">The type of event that was triggered</param>
        /// <param name="friendly">The friendly unity (if any)</param>
        /// <param name="enemy">The enemy unit (if any)</param>
        void onForwardEvent(EffectTrigger trigger, ICombatable friendly, ICombatable enemy);
        
        /// <summary>
        /// Triggers the backwards event effect to any listeners.
        /// </summary>
        /// <param name="trigger">The type of event that was triggered</param>
        /// <param name="friendly">The friendly unity (if any)</param>
        /// <param name="enemy">The enemy unit (if any)</param>
        void onBackwardsEvent(EffectTrigger trigger, ICombatable friendly, ICombatable enemy);
    }
}