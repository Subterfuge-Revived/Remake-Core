using System.Collections.Generic;

namespace Subterfuge.Remake.Core.Entities.Specialists.Effects
{
    public interface ISpecialistEffect
    {
        /// <summary>
        /// Applies the effect's forward actions
        /// </summary>
        /// <param name="state">The game state to get the deltas for</param>
        /// <param name="friendly">The friendly participant. Null if none.</param>
        /// <param name="enemy">The enemy participant. Null if none.</param>
        List<EffectDelta> GetForwardEffectDeltas(GameState.GameState state, Entity friendly, Entity enemy);
        
        /// <summary>
        /// Applies the backwards specialist effect
        /// </summary>
        /// <param name="state">The game state to get the deltas for</param>
        /// <param name="friendly">The friendly participant. Null if none.</param>
        /// <param name="enemy">The enemy participant. Null if none.</param>
        List<EffectDelta> GetBackwardEffectDeltas(GameState.GameState state, Entity friendly, Entity enemy);
    }
}