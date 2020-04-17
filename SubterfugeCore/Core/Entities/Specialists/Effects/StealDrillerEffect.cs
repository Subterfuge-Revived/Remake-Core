using System;
using System.Collections.Generic;
using System.Text;
using SubterfugeCore.Core.Entities.Specialists.Effects.Enums;
using SubterfugeCore.Core.Entities.Specialists.Effects.Exceptions;
using SubterfugeCore.Core.Interfaces;

namespace SubterfugeCore.Core.Entities.Specialists.Effects
{
    class StealDrillerEffect : ISpecialistEffect
    {
        private float _stealPercent;
        private EffectTrigger _trigger;
        private EffectTriggerRange _effectTriggerRange;
        private EffectTarget _effectTarget;
        
        // List to keep track of all stolen drillers.
        // Need to know previous steal amounts so that the action can be undone
        private Stack<int> stealHistory = new Stack<int>();


        public StealDrillerEffect(float stealPercent, EffectTrigger trigger)
        {
            this._stealPercent = stealPercent;
            this._trigger = trigger;
            this._effectTarget = EffectTarget.Enemy;
            this._effectTriggerRange = EffectTriggerRange.Local;
        }

        public void ForwardEffect(ICombatable friendly, ICombatable enemy)
        {
            int stolenDrillers = (int) (enemy.GetDrillerCount() * _stealPercent);
            enemy.RemoveDrillers(stolenDrillers);
            friendly.AddDrillers(stolenDrillers);
            this.stealHistory.Push(stolenDrillers);
        }

        public void BackwardEffect(ICombatable friendly, ICombatable enemy)
        {
            if (stealHistory.Count <= 0)
            {
                throw new SpecialistException();
            }
            // Get the last steal amount:
            int lastSteal = stealHistory.Pop();
            // Can't do this. Need an event that stores the initial steal amount O_O ...
            // enemy.addDrillers(drillerCount * stealPercent);
        }

        public EffectTrigger GetEffectTrigger()
        {
            return this._trigger;
        }

        public void SetEffectTrigger(EffectTrigger effectTrigger)
        {
            this._trigger = effectTrigger;
        }

        public EffectTarget GetEffectTarget()
        {
            return this._effectTarget;
        }

        public void SetEffectTarget(EffectTarget effectTarget)
        {
            this._effectTarget = effectTarget;
        }

        public EffectTriggerRange GetEffectType()
        {
            return this._effectTriggerRange;
        }

        public void SetEffectType(EffectTriggerRange effectTriggerRange)
        {
            this._effectTriggerRange = effectTriggerRange;
        }
    }
}
