using System.Collections.Generic;
using System.Linq;
using SubterfugeCore.Core.Components;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.Entities.Specialists.Effects.Enums;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Players;

namespace SubterfugeCore.Core.Entities.Specialists.Effects
{
    public class NumericSpecialistEffect : SpecialistEffect
    {
        /// <summary>
        /// The base value of the effect. If the effect value is scaled, this value will be multiplied by the scale.
        /// </summary>
        public float _effectValue { private get; set; } = 0;

        /// <summary>
        /// What should be effected by the change.
        /// </summary>
        public EffectModifier Effector { get; set; } = EffectModifier.None;

        /// <summary>
        /// If scaling should be applied, this object determines how the scaling is to be calculated. 
        /// </summary>
        public SpecialistEffectScale EffectScale { get; set; } = null;

        /// <summary>
        /// Determine the value of the forward effect to apply. If the effect is a scaling effect, the effect should
        /// follow the formula end = start * effectValue. If no scaling is to be applied, the effect should follow the formula
        /// end = start + effectValue.
        /// </summary>
        /// <param name="startValue">The starting value before the effect is applied.</param>
        /// <param name="friendly">The friendly participant. Null if none.</param>
        /// <param name="enemy">The enemy participant. Null if none.</param>
        /// <returns></returns>
        public float getForwardEffectDelta(GameState state, int startValue, Entity friendly, Entity enemy)
        {
            if (EffectScale == null)
            {
                return _effectValue;
            }
            // difference between result & start value if scaling.
            return startValue - (_effectValue * EffectScale.GetEffectScalar(state, friendly, enemy));
        }

        /// <summary>
        /// Determine the value of the backward effect to apply. If the effect is a scaling effect, the effect should
        /// follow the formula start = end / effectValue. If no scaling is to be applied, the effect should follow the formula
        /// start = end - effectValue.
        /// </summary>
        /// <param name="endValue">The ending value to revert change for</param>
        /// <param name="friendly">The friendly participant. Null if none.</param>
        /// <param name="enemy">The enemy participant. Null if none.</param>
        /// <returns></returns>
        public float getBackwardsEffectDelta(GameState state, int endValue, Entity friendly, Entity enemy)
        {
            if (EffectScale == null)
            {
                return -1 * _effectValue;
            }
            // Difference between end value and the result.
            return endValue - (EffectScale.GetEffectScalar(state, friendly, enemy) / _effectValue);
        }
        
        /// <summary>
        /// Gets the forwards effect deltas for a numerical specialist effect.
        /// </summary>
        /// <param name="state">The game state to get the effects for.</param>
        /// <param name="friendly">The original friendly triggering the event (if any)</param>
        /// <param name="enemy">The enemy participating in the event (if any)</param>
        /// <returns>A list of effect deltas to be applied</returns>
        public override List<EffectDelta> GetForwardEffectDeltas(GameState state, Entity friendly, Entity enemy)
        {
            List<IEntity> targets = this.getEffectTargets(state, friendly, enemy);
            List<EffectDelta> deltas = new List<EffectDelta>();

            foreach (IEntity target in targets)
            {
                int friendlyDelta = (int)this.getForwardEffectDelta(state,friendly.GetComponent<DrillerCarrier>().GetDrillerCount(), friendly, enemy);
                int enemyDelta = (int)this.getForwardEffectDelta(state,enemy.GetComponent<DrillerCarrier>().GetDrillerCount(), friendly, enemy);

                if (target.GetComponent<DrillerCarrier>().GetOwner() == friendly.GetComponent<DrillerCarrier>().GetOwner())
                {
                    deltas.Add(new EffectDelta(friendlyDelta, target, Effector));
                }
                else
                {
                    deltas.Add(new EffectDelta(enemyDelta, target, Effector));
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
        public override List<EffectDelta> GetBackwardEffectDeltas(GameState state, Entity friendly, Entity enemy)
        {
            List<IEntity> targets = this.getEffectTargets(state, friendly, enemy);
            List<EffectDelta> deltas = new List<EffectDelta>();

            foreach (IEntity target in targets)
            {
                int friendlyDelta = (int)this.getBackwardsEffectDelta(state,friendly.GetComponent<DrillerCarrier>().GetDrillerCount(), friendly, enemy);
                int enemyDelta = (int)this.getBackwardsEffectDelta(state,enemy.GetComponent<DrillerCarrier>().GetDrillerCount(), friendly, enemy);

                if (target.GetComponent<DrillerCarrier>().GetOwner() == friendly.GetComponent<DrillerCarrier>().GetOwner())
                {
                    deltas.Add(new EffectDelta(friendlyDelta, target, Effector));
                }
                else
                {
                    deltas.Add(new EffectDelta(enemyDelta, target, Effector));
                }
            }

            return deltas;
        }
    }
}