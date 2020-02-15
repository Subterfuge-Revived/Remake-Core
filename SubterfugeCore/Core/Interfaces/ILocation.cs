using System.Numerics;

namespace SubterfugeCore.Core.Interfaces
{
    /// <summary>
    /// Anything that has a location
    /// </summary>
    public interface ILocation
    {
        /// <summary>
        /// Get the current location
        /// </summary>
        /// <returns>The current location</returns>
        Vector2 getCurrentLocation();
    }
}
