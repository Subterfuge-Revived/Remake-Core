using System;

namespace SubterfugeCore.Core.Network
{
    /// <summary>
    /// Class to represent a network error
    /// </summary>
    [Serializable]
    public class NetworkError
    {
        /// <summary>
        /// The generic error message
        /// </summary>
        public string Message { get; set; }
    }
}