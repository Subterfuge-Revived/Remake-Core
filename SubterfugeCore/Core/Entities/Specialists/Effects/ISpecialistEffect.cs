using SubterfugeCore.Core.Interfaces.Outpost;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public interface ISpecialistEffect
    {
        void forwardEffect(ICombatable friendly, ICombatable enemy);
        void backwardEffect(ICombatable friendly, ICombatable enemy);
        EffectTrigger getEffectTrigger();
    }
}
