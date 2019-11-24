using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeCore.Core.Entities.Specialists.Effects
{
    class SpecialistEffectListener
    {

        public void invoke(EffectTrigger trigger)
        {
            switch (trigger) {
                case EffectTrigger.GLOBAL_COMBAT_LOSS:
                    break;
            }
        }

    }
}
