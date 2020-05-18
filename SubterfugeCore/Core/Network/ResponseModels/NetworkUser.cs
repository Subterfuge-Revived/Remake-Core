using SubterfugeCore.Core.Players;

namespace SubterfugeCore.Core.Network
{
    /// <summary>
    /// Representation of a user when returned from the Network.
    /// Can easily convert this type of user into a Player with `new Player(networkUser)`.
    /// </summary>
    public class NetworkUser : BaseNetworkResponse
    {
        /// <summary>
        /// The user's id
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// The user's name.
        /// </summary>
        public string Name { get; set; }
    }
}