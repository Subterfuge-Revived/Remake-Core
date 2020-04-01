using System;
using System.Numerics;

namespace SubterfugeCore.Core.Interfaces
{
    /// <summary>
    /// Anything that can be targeted by a sub
    /// </summary>
    public interface ITargetable : ILocation
    {
        /// <summary>
        /// Returns the combat location when being targeted from the specified location and speed.
        /// </summary>
        /// <param name="targetFrom">The location this object is being targeted from.</param>
        /// <param name="speed">The speed the targeting object has.</param>
        /// <returns>The combat location</returns>
        Vector2 getTargetLocation(Vector2 targetFrom, double speed);

        int getId();
    }
}
