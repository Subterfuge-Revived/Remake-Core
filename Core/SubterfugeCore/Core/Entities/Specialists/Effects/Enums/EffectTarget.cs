﻿namespace SubterfugeCore.Core.Entities.Specialists.Effects.Enums
{
    public enum EffectTarget
    {
        /// <summary>
        /// No effect targets 
        /// </summary>
        None,
        
        /// <summary>
        /// Effect will target friendly targets
        /// </summary>
        Friendly,
        
        /// <summary>
        /// Effect will target enemy targets
        /// </summary>
        Enemy,
        
        /// <summary>
        /// Effect will target both friendly and enemy targets but only consider combat participants
        /// </summary>
        Both,
        
        /// <summary>
        /// Effect will target any player regardless of if they are a combat participant.
        /// </summary>
        All,
    }
}