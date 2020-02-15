
using SubterfugeCore.Core.Entities.Locations;
using SubterfugeCore.Core.GameEvents;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Entities;
using SubterfugeCore.GameEvents;
using SubterfugeCore.Timing;
using System.Collections.Generic;

namespace SubterfugeCore
{
    /**
     * This game server class will hold all of the game logic.
     * This includes holding the game state, as well as being able to interpolate the locations of all a player's outposts, 
     * subs, etc.
     * 
     * No graphics will be used within this project. The graphics engine will need to reference the objects within this class to
     * determine how to draw.
     */
    public class Game
    {
        // Globally accessible time machine reference
        public static TimeMachine timeMachine;

        // temporary control for time machine.
        private bool forward = true;
        private bool addedEvents = false;

        // Creates an empty game with no players, no outposts.
        public Game()
        {
            // Creates a new game state and makes a time machine to reference the state
            GameState state = new GameState();
            timeMachine = new TimeMachine(state);
        }

        public Game(GameConfiguration gameConfiguration)
        {
            // Creates a new game state and makes a time machine to reference the state
            GameState state = new GameState();
            timeMachine = new TimeMachine(state);

            // Creates the map generator with a random seed
            MapGenerator mapGenerator = new MapGenerator(gameConfiguration);
            
            // Generate the map
            List<Outpost> outpostsToGenerate = mapGenerator.GenerateMap();
            
            // Add factory driller production events to the time machine.
            // TODO: Make this better.
            foreach(Outpost o in outpostsToGenerate)
            {
                if (o.getOutpostType() == OutpostType.FACTORY) {
                    timeMachine.addEvent(new FactoryProduceDrillersEvent(o, state.getCurrentTick().advance(36)));
                }
            }
            
            // Add the outposts to the map
            state.getOutposts().AddRange(outpostsToGenerate);
        }

    }
}
