namespace SubterfugeCore.Core.Network
{
    /// <summary>
    /// Class is parsed in Unity by using JsonConvert.Deserialize() on the Register Network response.
    /// For some reason including the JSON library in the dll to do it here throws errors.
    /// </summary>
    public class RegisterResponse : BaseNetworkResponse
    {
        /// <summary>
        /// The user that was registered (if any)
        /// </summary>
        public NetworkUser User { get; set; }
        
        /// <summary>
        /// The user's session id for future API calls
        /// </summary>
        public string Token { get; set; }
    }
}