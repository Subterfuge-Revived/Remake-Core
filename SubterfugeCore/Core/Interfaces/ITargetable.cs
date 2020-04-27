using System;
using System.Numerics;
using SubterfugeCore.Core.Topologies;

namespace SubterfugeCore.Core.Interfaces
{
    /// <summary>
    /// Anything that can be targeted by a sub
    /// </summary>
    public interface ITargetable : IPosition
    {
        /// <summary>
        /// Returns the combat location when being targeted from the specified location and speed.
        /// </summary>
        /// <param name="targetFrom">The location this object is being targeted from.</param>
        /// <param name="speed">The speed the targeting object has.</param>
        /// <returns>The combat location</returns>
        RftVector GetInterceptionPoint(RftVector targetFrom, float speed);

        int GetId();
    }
}
