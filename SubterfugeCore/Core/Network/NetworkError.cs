using System.Collections.Generic;
using System.ComponentModel.Design;

namespace SubterfugeCore.Core.Network
{
    /// <summary>
    /// Class to represent a network error
    /// </summary>
    public class NetworkError
    {
        /// <summary>
        /// The generic error message
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// A list of specific errors that occurred.
        /// </summary>
        public List<List<string>> Errors { get; set; }
    }
}