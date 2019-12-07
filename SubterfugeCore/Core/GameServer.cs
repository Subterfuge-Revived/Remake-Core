
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
     * No graphics will be used within this object. The graphics engine will need to reference the objects within this class to
     * determine how to draw.
     */
    public class GameServer
    {
        public static TimeMachine timeMachine;

        // temporary control for time machine.
        private bool forward = true;
        private bool addedEvents = false;

        public GameServer()
        {
            // Temporary. This will probably use some .fromJson methods.
            this.newGame();
            /*
            Outpost outpost1 = timeMachine.getState().getOutposts()[0];
            Outpost outpost2 = timeMachine.getState().getOutposts()[1];
            Outpost outpost3 = timeMachine.getState().getOutposts()[2];

            // Temporary. Sets up some initial time machine events
            LaunchEvent launchEvent1 = new LaunchEvent(timeMachine.getState().getCurrentTick().advance(100), outpost1, 30, outpost2);
            timeMachine.addEvent(launchEvent1);

            LaunchEvent launchEvent2 = new LaunchEvent(timeMachine.getState().getCurrentTick().advance(101), outpost2, 1, outpost1);
            timeMachine.addEvent(launchEvent2);

            LaunchEvent launchEvent3 = new LaunchEvent(timeMachine.getState().getCurrentTick().advance(450), outpost3, 30, outpost2);
            timeMachine.addEvent(launchEvent3);

            LaunchEvent launchEvent4 = new LaunchEvent(timeMachine.getState().getCurrentTick().advance(250), outpost1, 5, outpost3);
            timeMachine.addEvent(launchEvent4);

            LaunchEvent launchEvent5 = new LaunchEvent(timeMachine.getState().getCurrentTick().advance(101), outpost2, 1, outpost3);
            timeMachine.addEvent(launchEvent5);

            LaunchEvent launchEvent6 = new LaunchEvent(timeMachine.getState().getCurrentTick().advance(150), outpost3, 1, outpost2);
            timeMachine.addEvent(launchEvent6);
            */
        }

        // Creates a new game.
        public void newGame()
        {
            // Will do map generation here.
            GameState state = new GameState();
            timeMachine = new TimeMachine(state);

            MapGenerator mapGenerator = new MapGenerator(45454545);  // send a random seed.
            mapGenerator.SetData(state.getPlayers().Count, 5, 5, 75, 250);  // Setup the map generation parameters
            List<Outpost> outpostsToGenerate = mapGenerator.GenerateMap();
            foreach(Outpost o in outpostsToGenerate)
            {
                if (o.getOutpostType() == OutpostType.FACTORY) {
                    timeMachine.addEvent(new FactoryProduceDrillersEvent(o, state.getCurrentTick().advance(36)));
                }
            }
            state.getOutposts().AddRange(outpostsToGenerate); // Generate the map and add the returned list of outposts to the map!

        }

    }
}
