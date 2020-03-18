using System.Collections.Generic;

namespace SubterfugeCore.Core.Network
{
    /// <summary>
    /// Class is parsed in Unity by using JsonConvert.Deserialize() on Network responses.
    /// For some reason including the JSON library in the dll to do it here throws errors.
    /// </summary>
    public class GameRoom
    {
        /// <summary>
        /// The id of the game room
        /// </summary>
        public int room_id { get; set; }
        
        /// <summary>
        /// The playerId of who created the game room
        /// </summary>
        public int creator_id { get; set; }
        
        /// <summary>
        /// If the game is ranked or not
        /// </summary>
        public bool rated { get; set; }
        
        /// <summary>
        /// How many players are currently in the game's lobby
        /// </summary>
        public int player_count { get; set; }
        
        /// <summary>
        /// The lowest rating required to join the game lobby
        /// </summary>
        public int min_rating { get; set; }
        
        /// <summary>
        /// The room's description
        /// </summary>
        public string description { get; set; }
        
        /// <summary>
        /// The type of game (domination, mining, etc.)
        /// </summary>
        public int goal { get; set; }
        
        /// <summary>
        /// If the game is anonymous
        /// </summary>
        public bool anonimity { get; set; }
        
        /// <summary>
        /// What map/theme of game the users are playing on
        /// </summary>
        public int map { get; set; }
        
        /// <summary>
        /// The map's seed
        /// </summary>
        public int seed { get; set; }
        
        /// <summary>
        /// A list of all players currently in the lobby
        /// </summary>
        public List<NetworkUser> players { get; set; }
    }
}