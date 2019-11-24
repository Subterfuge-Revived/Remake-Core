using SubterfugeCore.Core.Interfaces.Outpost;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public interface ISpecialistEffect
    {
        void forwardEffect(ICombatable combatant);
        void backwardEffect(ICombatable combatant);
        EffectTrigger getEffectTrigger();
    }
}
