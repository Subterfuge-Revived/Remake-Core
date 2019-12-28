using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeCore.Core.Interfaces.Outpost
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
