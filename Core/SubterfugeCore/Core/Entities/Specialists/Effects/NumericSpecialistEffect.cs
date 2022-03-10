using System.Collections.Generic;
using SubterfugeCore.Core.Components;

namespace SubterfugeCore.Core.Entities.Specialists.Effects
{
    public class NumericSpecialistEffect : SpecialistEffect
    {

        /// <summary>
        /// Determine the value of the forward effect to apply. If the effect is a scaling effect, the effect should
        /// follow the formula end = start * effectValue. If no scaling is to be applied, the effect should follow the formula
        /// end = start + effectValue.
        /// </summary>
        /// <param name="state">Gamestate</param>
        /// <param name="startValue">The starting value before the effect is applied.</param>
        /// <param name="friendly">The friendly participant. Null if none.</param>
        /// <param name="enemy">The enemy participant. Null if none.</param>
        /// <returns></returns>
        private float GetForwardEffectDelta(GameState.GameState state, int startValue, Entity friendly, Entity enemy)
        {
            if (configuration.EffectScale == null)
            {
                return configuration.Value;
            }
            // difference between result & start value if scaling.
            return startValue - (configuration.Value * configuration.EffectScale.GetEffectScalar(state, friendly, enemy));
        }

        /// <summary>
        /// Determine the value of the backward effect to apply. If the effect is a scaling effect, the effect should
        /// follow the formula start = end / effectValue. If no scaling is to be applied, the effect should follow the formula
        /// start = end - effectValue.
        /// </summary>
        /// <param name="state">The gamestate</param>
        /// <param name="endValue">The ending value to revert change for</param>
        /// <param name="friendly">The friendly participant. Null if none.</param>
        /// <param name="enemy">The enemy participant. Null if none.</param>
        /// <returns></returns>
        public float GetBackwardsEffectDelta(GameState.GameState state, int endValue, Entity friendly, Entity enemy)
        {
            if (configuration.EffectScale == null)
            {
                return -1 * configuration.Value;
            }
            // Difference between end value and the result.
            return endValue - (configuration.EffectScale.GetEffectScalar(state, friendly, enemy) / configuration.Value);
        }
        
        /// <summary>
        /// Gets the forwards effect deltas for a numerical specialist effect.
        /// </summary>
        /// <param name="state">The game state to get the effects for.</param>
        /// <param name="friendly">The original friendly triggering the event (if any)</param>
        /// <param name="enemy">The enemy participating in the event (if any)</param>
        /// <returns>A list of effect deltas to be applied</returns>
        public override List<EffectDelta> GetForwardEffectDeltas(GameState.GameState state, Entity friendly, Entity enemy)
        {
            List<IEntity> targets = this.GetEffectTargets(state, friendly, enemy);
            List<EffectDelta> deltas = new List<EffectDelta>();

            foreach (IEntity target in targets)
            {
                int friendlyDelta = (int)this.GetForwardEffectDelta(state,friendly.GetComponent<DrillerCarrier>().GetDrillerCount(), friendly, enemy);
                int enemyDelta = (int)this.GetForwardEffectDelta(state,enemy.GetComponent<DrillerCarrier>().GetDrillerCount(), friendly, enemy);

                if (target.GetComponent<DrillerCarrier>().GetOwner() == friendly.GetComponent<DrillerCarrier>().GetOwner())
                {
                    deltas.Add(new EffectDelta(friendlyDelta, target, configuration.EffectModifier));
                }
                else
                {
                    deltas.Add(new EffectDelta(enemyDelta, target, configuration.EffectModifier));
                }
                
            }

            return deltas;
        }
        
        /// <summary>
        /// Determines the backwards effect deltas to apply in an effect
        /// </summary>
        /// <param name="state">The game state to get the effects for.</param>
        /// <param name="friendly">The friendly triggering the event (if any)</param>
        /// <param name="enemy">The enemy participating in the event (if any)</param>
        /// <returns></returns>
        public override List<EffectDelta> GetBackwardEffectDeltas(GameState.GameState state, Entity friendly, Entity enemy)
        {
            List<IEntity> targets = this.GetEffectTargets(state, friendly, enemy);
            List<EffectDelta> deltas = new List<EffectDelta>();

            foreach (IEntity target in targets)
            {
                int friendlyDelta = (int)this.GetBackwardsEffectDelta(state,friendly.GetComponent<DrillerCarrier>().GetDrillerCount(), friendly, enemy);
                int enemyDelta = (int)this.GetBackwardsEffectDelta(state,enemy.GetComponent<DrillerCarrier>().GetDrillerCount(), friendly, enemy);

                if (target.GetComponent<DrillerCarrier>().GetOwner() == friendly.GetComponent<DrillerCarrier>().GetOwner())
                {
                    deltas.Add(new EffectDelta(friendlyDelta, target, configuration.EffectModifier));
                }
                else
                {
                    deltas.Add(new EffectDelta(enemyDelta, target, configuration.EffectModifier));
                }
            }

            return deltas;
        }
    }
}