using System;
using System.Collections.Generic;
using System.Linq;
using SubterfugeCore.Core.Entities.Specialists.Effects;
using SubterfugeCore.Core.Players;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.Entities.Specialists
{
    /// <summary>
    /// Base class for a specialist.
    /// </summary>
    public class Specialist
    {
        private SpecialistConfiguration _specialistConfiguration;
        private Player _owner;
        private bool _isCaptured;

        /// <summary>
        /// Abstract constructor for a specialist. All inherited specialist classes require implementing this.
        /// </summary>
        /// <param name="name">The name of the specialist</param>
        /// <param name="priority">The specialist priority</param>
        /// <param name="owner">The player that owns the specialist</param>
        public Specialist(Player owner, SpecialistConfiguration configuration)
        {
            _specialistConfiguration = configuration;
            _owner = owner;
        }
        
        /// <summary>
        /// Returns a list of specialist effects that the specialist possesses.
        /// </summary>
        /// <returns>A list of specialist effects that the specialist can apply</returns>
        public List<ISpecialistEffect> GetSpecialistEffects()
        {
            return _specialistConfiguration.SpecialistEffects
                .Select(effect => new SpecialistEffectFactory().CreateSpecialistEffect(effect))
                .ToList();
        }

        /// <summary>
        /// Applies the specialist's effects to a friendly and enemy combatable.
        /// </summary>
        /// <param name="state">The game state</param>
        /// <param name="friendly">The friendly combatable to effect</param>
        /// <param name="enemy">The enemy combatable to effect</param>
        public void ApplyEffect(GameState.GameState state, Entity friendly, Entity enemy)
        {
            foreach(var specialistEffect in GetSpecialistEffects())
            {
                var effect = (SpecialistEffect)specialistEffect;
                effect.GetForwardEffectDeltas(state, friendly, enemy);
            }
        }

        /// <summary>
        /// Reverses a specialist effect to a friendly and enemy combatable
        /// </summary>
        /// <param name="state">Gamestate</param>
        /// <param name="friendly">The friendly combatable to reverse effects to</param>
        /// <param name="enemy">The enemy combatable to reverse effects to</param>
        public void UndoEffect(GameState.GameState state, Entity friendly, Entity enemy)
        {
            foreach (var specialistEffect in GetSpecialistEffects())
            {
                var effect = (SpecialistEffect)specialistEffect;
                effect.GetBackwardEffectDeltas(state, friendly, enemy);
            }
        }

        /// <summary>
        /// Returns the specialist's priority.
        /// </summary>
        /// <returns>The combat priority of the specialist</returns>
        public int GetPriority()
        {
            return _specialistConfiguration.Priority;
        }

        /// <summary>
        /// Returns the specialist id.
        /// </summary>
        /// <returns>The specialist's id</returns>
        public string GetId()
        {
            return _specialistConfiguration.Id;
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
            foreach (var specialistEffect1 in GetSpecialistEffects())
            {
                var specialistEffect = (SpecialistEffect)specialistEffect1;
                if (specialistEffect.configuration.EffectTrigger == trigger)
                {
                    // specialistEffect.forwardEffect();
                }
            }
        }

        /// <summary>
        /// Captures the specialist
        /// </summary>
        /// <param name="isCaptured">Sets the specialist captured state</param>
        public void SetCaptured(bool isCaptured)
        {
            this._isCaptured = isCaptured;
        }

        /// <summary>
        /// Check if the specialist is captured
        /// </summary>
        /// <returns>If the specialist if captured</returns>
        public bool IsCaptured()
        {
            return _isCaptured;
        }
    }
}