using System.Numerics;
using SubterfugeCore.Core.Topologies;

namespace SubterfugeCore.Core.Interfaces
{
    /// <summary>
    /// Anything that has a location
    /// </summary>
    public interface IPosition
    {
        /// <summary>
        /// Get the current location
        /// </summary>
        /// <returns>The current location</returns>
        RftVector GetCurrentPosition();
    }
}
