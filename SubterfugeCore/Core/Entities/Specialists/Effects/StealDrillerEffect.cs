using System;
using System.Collections.Generic;
using System.Text;
using SubterfugeCore.Core.Interfaces;

namespace SubterfugeCore.Core.Entities.Specialists.Effects
{
    class StealDrillerEffect : ISpecialistEffect
    {
        private float _stealPercent;
        EffectTrigger _trigger;

        public StealDrillerEffect(float stealPercent, EffectTrigger trigger)
        {
            this._stealPercent = stealPercent;
            this._trigger = trigger;
        }

        public void ForwardEffect(ICombatable friendly, ICombatable enemy)
        {
            enemy.RemoveDrillers((int)(enemy.GetDrillerCount() * _stealPercent));
        }

        public void BackwardEffect(ICombatable friendly, ICombatable enemy)
        {
            // Can't do this. Need an event that stores the initial steal amount O_O ...
            // enemy.addDrillers(drillerCount * stealPercent);
        }

        public EffectTrigger GetEffectTrigger()
        {
            return this._trigger;
        }
    }
}
