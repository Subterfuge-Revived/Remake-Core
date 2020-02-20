namespace SubterfugeCore.Core.Network
{
    /// <summary>
    /// Base Network response. All Network responses should have a 'success' variable to determine if the operation
    /// succeeded and a message describing any issues if it didn't
    /// </summary>
    public class NetworkResponse
    {
        /// <summary>
        /// If the network operation was successful
        /// </summary>
        public bool success { get; set; }
        
        /// <summary>
        /// Details about the network operation
        /// </summary>
        public string message { get; set; }
    }
}