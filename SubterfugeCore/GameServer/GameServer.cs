
namespace SubterfugeCore
{
    /**
     * This game server class will hold all of the game logic.
     * This includes holding the game state, as well as being able to interpolate the locations of all a player's outposts, 
     * subs, etc.
     * 
     * No graphics will be used within this object. The graphics engine will need to reference the objects within this class to
     * determine how to draw.
     */
    public class GameServer
    {
        private GameState state = new GameState();

        public GameState GetGameState()
        {
            // Update the game state before sending it back
            state.update();
            return state;
        }       
    }
}
