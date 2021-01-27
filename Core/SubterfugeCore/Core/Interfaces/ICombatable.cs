using System;
using SubterfugeCore.Core.Players;

namespace SubterfugeCore.Core.Interfaces
{
    /// <summary>
    /// Anything that can perform combat
    /// </summary>
    public interface ICombatable : IDrillerCarrier, ISpecialistCarrier, ITargetable
    {
        
    }
}
