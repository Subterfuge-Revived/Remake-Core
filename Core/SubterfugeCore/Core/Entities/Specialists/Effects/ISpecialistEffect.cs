using System.Collections.Generic;
using SubterfugeCore.Core.Entities.Specialists.Effects.Enums;
using SubterfugeCore.Core.Interfaces;

namespace SubterfugeCore.Core.Entities.Specialists.Effects
{
    public interface ISpecialistEffect
    {
        /// <summary>
        /// Applies the effect's forward actions
        /// </summary>
        /// <param name="state">The game state to get the deltas for</param>
        /// <param name="friendly">The friendly participant. Null if none.</param>
        /// <param name="enemy">The enemy participant. Null if none.</param>
        List<EffectDelta> GetForwardEffectDeltas(GameState state, Entity friendly, Entity enemy);
        
        /// <summary>
        /// Applies the backwards specialist effect
        /// </summary>
        /// <param name="state">The game state to get the deltas for</param>
        /// <param name="friendly">The friendly participant. Null if none.</param>
        /// <param name="enemy">The enemy participant. Null if none.</param>
        List<EffectDelta> GetBackwardEffectDeltas(GameState state, Entity friendly, Entity enemy);
    }
}