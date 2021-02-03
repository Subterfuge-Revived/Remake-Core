using System;
using System.Numerics;
using SubterfugeCore.Core.Timing;
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
        RftVector GetInterceptionPosition(RftVector targetFrom, float speed);
        
        /// <summary>
        /// If you are targetable, you should be moving at some speed.
        /// </summary>
        /// <returns>Object speed</returns>
        float GetSpeed();
        
        /// <summary>
        /// Get the direction you are travelling
        /// </summary>
        /// <returns>The direction the object is travelling</returns>
        Vector2 GetDirection();

        /// <summary>
        /// The gameTick when the object is expected to arrive at its destination (if any)
        /// </summary>
        /// <returns>Arrival time</returns>
        GameTick GetExpectedArrival();
    }
}
