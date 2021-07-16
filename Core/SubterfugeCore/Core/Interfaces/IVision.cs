using System;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Core.Topologies;

namespace SubterfugeCore.Core.Interfaces
{
    public interface IVision : ICombatable
    {
        /// <summary>
        /// Gets the range of vision for the current object
        /// </summary>
        /// <returns>The object's vision range</returns>
        float GetVisionRange();


        /// <summary>
        /// Determines if the position is in vision of this object.
        /// </summary>
        /// <param name="position">The position to check</param>
        /// <returns>If the object is in the vision range.</returns>
        bool isInVisionRange(GameTick tick, IPosition position);

    }
}