using SubterfugeCore.Components;
using SubterfugeCore.Core.Components.Outpost;
using SubterfugeCore.Core.Entities.Specialists;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeCore.Core.Interfaces.Outpost
{
    /// <summary>
    /// Anything that is able to carry specialists
    /// </summary>
    public interface ISpecialistCarrier : ILocation, IOwnable
    {
        /// <summary>
        /// Returns the specialist manager for the object.
        /// </summary>
        /// <returns>The specialist manager</returns>
        SpecialistManager getSpecialistManager();
    }
}
