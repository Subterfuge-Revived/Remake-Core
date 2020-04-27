using System.Collections.Generic;
using SubterfugeCore.Core.Entities.Specialists.Effects.Enums;
using SubterfugeCore.Core.Interfaces;

namespace SubterfugeCore.Core.Entities.Specialists.Effects
{
    /// <summary>
    /// An effect to alter the number of drillers at a location.
    /// </summary>
    public class AlterDrillerEffect : NumericSpecialistEffect
    {
        public override List<EffectDelta> GetForwardEffectDeltas(ICombatable friendly, ICombatable enemy)
        {
            List<EffectDelta> deltas = new List<EffectDelta>();
            int friendlyDelta = (int)this.getForwardEffectDelta(friendly.GetDrillerCount(), friendly, enemy);
            int enemyDelta = (int)this.getForwardEffectDelta(enemy.GetDrillerCount(), friendly, enemy);
            
            switch(this._effectTarget)
            {
                case EffectTarget.Friendly:
                    deltas.Add(new EffectDelta(friendlyDelta, friendly, EffectEffector.Driller));
                    break;
                case EffectTarget.Enemy:
                    deltas.Add(new EffectDelta(enemyDelta, enemy, EffectEffector.Driller));
                    break;
                case EffectTarget.Both:
                case EffectTarget.All:
                    deltas.Add(new EffectDelta(friendlyDelta, friendly, EffectEffector.Driller));
                    deltas.Add(new EffectDelta(enemyDelta, enemy, EffectEffector.Driller));
                    break;
            }

            return deltas;
        }

        public override List<EffectDelta> GetBackwardEffectDeltas(ICombatable friendly, ICombatable enemy)
        {
            List<EffectDelta> deltas = new List<EffectDelta>();
            int friendlyDelta = (int)this.getBackwardsEffectDelta(friendly.GetDrillerCount(), friendly, enemy);
            int enemyDelta = (int)this.getBackwardsEffectDelta(enemy.GetDrillerCount(), friendly, enemy);
            
            switch(this._effectTarget)
            {
                case EffectTarget.Friendly:
                    deltas.Add(new EffectDelta(friendlyDelta, friendly, EffectEffector.Driller));
                    break;
                case EffectTarget.Enemy:
                    deltas.Add(new EffectDelta(enemyDelta, enemy, EffectEffector.Driller));
                    break;
                case EffectTarget.Both:
                case EffectTarget.All:
                    deltas.Add(new EffectDelta(friendlyDelta, friendly, EffectEffector.Driller));
                    deltas.Add(new EffectDelta(enemyDelta, enemy, EffectEffector.Driller));
                    break;
            }

            return deltas;
        }

    }
}