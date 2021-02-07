using System;
using System.Numerics;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Core.Topologies;

namespace SubterfugeCore.Core.Interfaces
{
    /// <summary>
    /// Anything that can perform combat
    /// </summary>
    public interface ICombatable : IDrillerCarrier, ISpecialistCarrier, ITargetable, IShieldable, ISubLauncher
    {
    }
}