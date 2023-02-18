using System;
using System.Collections.Generic;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Specialists.Effects;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Core.Entities.Specialists
{
    /// <summary>
    /// Base class for a specialist.
    /// </summary>
    public class Specialist
    {
        /// <summary>
        /// The specialist priority
        /// </summary>
        private readonly int _priority;
        
        /// <summary>
        /// The name of the specialist
        /// </summary>
        private string _specialistName;
        
        /// <summary>
        /// The name of the specialist
        /// </summary>
        /// TODO: Generate the id from a known seed.
        private readonly string _specialistId = Guid.NewGuid().ToString();
        
        /// <summary>
        /// The player who owns the specialist
        /// </summary>
        Player _owner;
        
        /// <summary>
        /// A list of specialist effects that the specialist can apply
        /// </summary>
        private readonly List<ISpecialistEffect> _specialistEffects = new List<ISpecialistEffect>();

        /// <summary>
        /// Is the specialist captured by another player?
        /// </summary>
        private bool _isCaptured;

        /// <summary>
        /// Abstract constructor for a specialist. All inherited specialist classes require implementing this.
        /// </summary>
        /// <param name="name">The name of the specialist</param>
        /// <param name="priority">The specialist priority</param>
        /// <param name="owner">The player that owns the specialist</param>
        public Specialist(string name, int priority, Player owner)
        {
            _specialistName = name;
            _priority = priority;
            _owner = owner;
        }
        
        /// <summary>
        /// Returns a list of specialist effects that the specialist possesses.
        /// </summary>
        /// <returns>A list of specialist effects that the specialist can apply</returns>
        public List<ISpecialistEffect> GetSpecialistEffects()
        {
            return this._specialistEffects;
        }

        /// <summary>
        /// Removes a specialist effect from the specailist
        /// </summary>
        /// <param name="effect">The specialist effect to remove.</param>
        public void RemoveSpecialistEffect(ISpecialistEffect effect)
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
        public void AddSpecialistEffect(ISpecialistEffect effect)
        {
            _specialistEffects.Add(effect);
        }

        /// <summary>
        /// Applies the specialist's effects to a friendly and enemy combatable.
        /// </summary>
        /// <param name="state">The game state</param>
        /// <param name="friendly">The friendly combatable to effect</param>
        /// <param name="enemy">The enemy combatable to effect</param>
        public void ApplyEffect(GameState.GameState state, Entity friendly, Entity enemy)
        {
            foreach(var specialistEffect in _specialistEffects)
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
            foreach (var specialistEffect in this._specialistEffects)
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
            return this._priority;
        }

        /// <summary>
        /// Returns the specialist id.
        /// </summary>
        /// <returns>The specialist's id</returns>
        public string GetId()
        {
            return _specialistId;
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
            foreach (var specialistEffect1 in _specialistEffects)
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