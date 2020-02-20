using System;

namespace SubterfugeCore.Core.Interfaces
{
    /// <summary>
    /// Anything that needs to be sent to the server or parsed from the server
    /// </summary>
    interface ISerializable
    {
        /// <summary>
        /// Converts the object to a string
        /// </summary>
        /// <returns>String representation</returns>
        String toJSON();
        
        /// <summary>
        /// Converts the object from a string to an object
        /// </summary>
        /// <param name="jsonString">The string to parse</param>
        void fromJSON(string jsonString);
    }
}
