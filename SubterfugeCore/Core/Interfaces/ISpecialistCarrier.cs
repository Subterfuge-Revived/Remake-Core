using SubterfugeCore.Components;
using SubterfugeCore.Core.Components.Outpost;
using SubterfugeCore.Core.Entities.Specialists;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeCore.Core.Interfaces.Outpost
{
    public interface ISpecialistCarrier : ILocation, IOwnable
    {
        SpecialistManager getSpecialistManager();
    }
}
