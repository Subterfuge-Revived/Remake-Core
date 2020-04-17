using SubterfugeCore.Core.Entities.Specialists.Effects.Enums;

namespace SubterfugeCore.Core.Entities.Specialists.Effects
{
    public class EffectFactory
    {
        public ISpecialistEffect createSpecialistEffect(EffectType type, int value, EffectTarget target, EffectTrigger trigger, EffectTriggerRange triggerRange)
        {
            SpecialistEffect effect = null;
            
            switch (type)
            {
                case EffectType.AlterDriller:
                    AlterDrillerEffect alterDrillerEffect = new AlterDrillerEffect();
                    effect = alterDrillerEffect;
                    break;
                case EffectType.StealDriller:
                    StealDrillerEffect stealDrillerEffect = new StealDrillerEffect(value, trigger);
                    effect = stealDrillerEffect;
                    break;
            }
            
            this.setEffectValues(effect, value, target, trigger, triggerRange);

            return effect;
        }

        private void setEffectValues(SpecialistEffect effect, int value, EffectTarget target, EffectTrigger trigger,
            EffectTriggerRange triggerRange)
        {
            effect._effectValue = value;
            effect._effectTarget = target;
            effect._effectTrigger = trigger;
            effect._effectTriggerRange = triggerRange;
        }
    }
}