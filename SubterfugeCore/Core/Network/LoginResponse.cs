namespace SubterfugeCore.Core.Network
{
    /// <summary>
    /// Class is parsed in Unity by using JsonConvert.Deserialize() on the Login Network response.
    /// For some reason including the JSON library in the dll to do it here throws errors.
    /// </summary>
    public class LoginResponse : NetworkResponse
    {
        /// <summary>
        /// The Player that just logged in.
        /// </summary>
        public NetworkUser user { get; set; }
        
        /// <summary>
        /// The user's session token to use in future API requests.
        /// </summary>
        public string token { get; set; }
    }
}