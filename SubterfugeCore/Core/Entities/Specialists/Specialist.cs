using System;
using System.Collections.Generic;
using SubterfugeCore.Core.Entities.Specialists.Effects;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Players;

namespace SubterfugeCore.Core.Entities.Specialists
{
    /// <summary>
    /// Base class for a specialist.
    /// </summary>
    public abstract class Specialist : IOwnable
    {
        /// <summary>
        /// The specialist priority
        /// </summary>
        int _priority;
        
        /// <summary>
        /// The name of the specialist
        /// </summary>
        String _specialistName;
        
        /// <summary>
        /// The player who owns the specialist
        /// </summary>
        Player _owner;
        
        /// <summary>
        /// A list of specialist effects that the specialist can apply
        /// </summary>
        List<SpecialistEffect> _specialistEffects = new List<SpecialistEffect>();

        /// <summary>
        /// Is the specialist captured by another player?
        /// </summary>
        public bool IsCaptured { get; set; } = false;

        /// <summary>
        /// Abstract constructor for a specialist. All inherited specialist classes require implementing this.
        /// </summary>
        /// <param name="name">The name of the specialist</param>
        /// <param name="priority">The specialist priority</param>
        /// <param name="owner">The player that owns the specialist</param>
        protected Specialist(String name, int priority, Player owner)
        {
            this._specialistName = name;
            this._priority = priority;
            this._owner = owner;
        }
        
        /// <summary>
        /// Returns a list of specialist effects that the specialist possesses.
        /// </summary>
        /// <returns>A list of specialist effects that the specialist can apply</returns>
        public List<SpecialistEffect> GetSpecialistEffects()
        {
            return this._specialistEffects;
        }

        /// <summary>
        /// Removes a specialist effect from the specailist
        /// </summary>
        /// <param name="effect">The specialist effect to remove.</param>
        public void RemoveSpecialistEffect(SpecialistEffect effect)
        {
            if(_specialistEffects.Contains(effect))
            {
                _specialistEffects.Remove(effect);
            }
        }

        /// <summary>
        /// Adds a specialist effect to the specialist
        /// </summary>
        /// <param name="effect">The effect to add.</param>
        public void AddSpecialistEffect(SpecialistEffect effect)
        {
            _specialistEffects.Add(effect);
        }

        /// <summary>
        /// Applies the specialist's effects to a friendly and enemy combatable.
        /// </summary>
        /// <param name="friendly">The friendly combatable to effect</param>
        /// <param name="enemy">The enemy combatable to effect</param>
        public void ApplyEffect(ICombatable friendly, ICombatable enemy)
        {
            foreach(SpecialistEffect effect in this._specialistEffects)
            {
                effect.GetForwardEffectDeltas(friendly, enemy);
            }
        }

        /// <summary>
        /// Reverses a specialist effect to a friendly and enemy combatable
        /// </summary>
        /// <param name="friendly">The friendly combatable to reverse effects to</param>
        /// <param name="enemy">The enemy combatable to reverse effects to</param>
        public void UndoEffect(ICombatable friendly, ICombatable enemy)
        {
            foreach (SpecialistEffect effect in this._specialistEffects)
            {
                effect.GetBackwardEffectDeltas(friendly, enemy);
            }
        }

        /// <summary>
        /// Gets a textual description of a specialist effect.
        /// </summary>
        /// <param name="effect">The specialist effect to textualize</param>
        /// <returns>Player friendly text that explains the result of this effect.</returns>
        public abstract string GetEffectAsText(ISpecialistEffect effect);

        /// <summary>
        /// Returns the specialist's priority.
        /// </summary>
        /// <returns>The combat priority of the specialist</returns>
        public int GetPriority()
        {
            return this._priority;
        }
        
        /// <summary>
        /// Gets the owner of the specialist.
        /// </summary>
        /// <returns>The specialist's owner</returns>
        public Player GetOwner()
        {
            return this._owner;
        }

        /// <summary>
        /// Sets the owner of the specialist
        /// </summary>
        /// <param name="newOwner">The new owner of the specialist</param>
        public void SetOwner(Player newOwner)
        {
            this._owner = newOwner;
        }

        /// <summary>
        /// invokes the effects of a specialist effect
        /// </summary>
        /// <param name="trigger">The event that triggered the effects to determine if effects should be triggered.</param>
        public void Invoke(EffectTrigger trigger)
        {
            // Loop through specialist effects.
            // Determine if the effect should be triggered.
            foreach (SpecialistEffect specialistEffect in this._specialistEffects)
            {
                if (specialistEffect._effectTrigger == trigger)
                {
                    // specialistEffect.forwardEffect();
                }
            }
        }
    }
}
