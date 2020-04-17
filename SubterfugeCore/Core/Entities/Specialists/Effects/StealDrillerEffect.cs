using System;
using System.Collections.Generic;
using System.Text;
using SubterfugeCore.Core.Entities.Specialists.Effects.Enums;
using SubterfugeCore.Core.Entities.Specialists.Effects.Exceptions;
using SubterfugeCore.Core.Interfaces;

namespace SubterfugeCore.Core.Entities.Specialists.Effects
{
    class StealDrillerEffect : SpecialistEffect
    {   
        // List to keep track of all stolen drillers.
        // Need to know previous steal amounts so that the action can be undone
        private Stack<int> stealHistory = new Stack<int>();


        public StealDrillerEffect(float stealPercent, EffectTrigger trigger)
        {
            this._effectValue = stealPercent;
            this._effectTrigger = trigger;
            this._effectTarget = EffectTarget.Enemy;
            this._effectTriggerRange = EffectTriggerRange.Local;
        }

        public override void ForwardEffect(ICombatable friendly, ICombatable enemy)
        {
            int stolenDrillers = (int) (enemy.GetDrillerCount() * getEffectValue(friendly, enemy));
            enemy.RemoveDrillers(stolenDrillers);
            friendly.AddDrillers(stolenDrillers);
            this.stealHistory.Push(stolenDrillers);
        }

        public override void BackwardEffect(ICombatable friendly, ICombatable enemy)
        {
            if (stealHistory.Count <= 0)
            {
                throw new SpecialistException();
            }
            // Get the last steal amount:
            int lastSteal = stealHistory.Pop();
            enemy.AddDrillers(lastSteal);
            friendly.RemoveDrillers(lastSteal);
        }
    }
}
