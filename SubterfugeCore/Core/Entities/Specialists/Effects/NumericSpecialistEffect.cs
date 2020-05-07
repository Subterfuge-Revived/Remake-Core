using System.Collections.Generic;
using System.Linq;
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
        public EffectEffector Effector { get; set; } = EffectEffector.None;

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
        public float getForwardEffectDelta(int startValue, ICombatable friendly, ICombatable enemy)
        {
            if (EffectScale == null)
            {
                return _effectValue;
            }
            // difference between result & start value if scaling.
            return startValue - (_effectValue * EffectScale.GetEffectScalar(friendly, enemy));
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
        public float getBackwardsEffectDelta(int endValue, ICombatable friendly, ICombatable enemy)
        {
            if (EffectScale == null)
            {
                return -1 * _effectValue;
            }
            // Difference between end value and the result.
            return endValue - (EffectScale.GetEffectScalar(friendly, enemy) / _effectValue);
        }
        
        /// <summary>
        /// Gets the forwards effect deltas for a numerical specialist effect.
        /// </summary>
        /// <param name="friendly">The original friendly triggering the event (if any)</param>
        /// <param name="enemy">The enemy participating in the event (if any)</param>
        /// <returns>A list of effect deltas to be applied</returns>
        public override List<EffectDelta> GetForwardEffectDeltas(ICombatable friendly, ICombatable enemy)
        {
            List<ICombatable> targets = this.getEffectTargets(friendly, enemy);
            List<EffectDelta> deltas = new List<EffectDelta>();

            foreach (ICombatable target in targets)
            {
                int friendlyDelta = (int)this.getForwardEffectDelta(friendly.GetDrillerCount(), friendly, enemy);
                int enemyDelta = (int)this.getForwardEffectDelta(enemy.GetDrillerCount(), friendly, enemy);

                if (target.GetOwner() == friendly.GetOwner())
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
        /// <param name="friendly">The friendly triggering the event (if any)</param>
        /// <param name="enemy">The enemy participating in the event (if any)</param>
        /// <returns></returns>
        public override List<EffectDelta> GetBackwardEffectDeltas(ICombatable friendly, ICombatable enemy)
        {
            List<ICombatable> targets = this.getEffectTargets(friendly, enemy);
            List<EffectDelta> deltas = new List<EffectDelta>();

            foreach (ICombatable target in targets)
            {
                int friendlyDelta = (int)this.getBackwardsEffectDelta(friendly.GetDrillerCount(), friendly, enemy);
                int enemyDelta = (int)this.getBackwardsEffectDelta(enemy.GetDrillerCount(), friendly, enemy);

                if (target.GetOwner() == friendly.GetOwner())
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