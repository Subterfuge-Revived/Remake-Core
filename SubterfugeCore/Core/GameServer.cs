
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Entities;
using SubterfugeCore.GameEvents;
using SubterfugeCore.Timing;

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
        public static GameState state;

        // temporary control for time machine.
        private bool forward = true;
        private bool addedEvents = false;

        public GameServer()
        {
            // Temporary. This will probably use some .fromJson methods.
            this.newGame();

            Outpost outpost1 = state.getOutposts()[0];
            Outpost outpost2 = state.getOutposts()[1];
            Outpost outpost3 = state.getOutposts()[2];

            // Temporary. Sets up some initial time machine events
            SubLaunchEvent launchEvent1 = new SubLaunchEvent(state.getCurrentTick().advance(100), outpost1, 30, outpost2);
            timeMachine.addEvent(launchEvent1);

            SubLaunchEvent launchEvent2 = new SubLaunchEvent(state.getCurrentTick().advance(101), outpost2, 1, outpost1);
            timeMachine.addEvent(launchEvent2);

            SubLaunchEvent launchEvent3 = new SubLaunchEvent(state.getCurrentTick().advance(101), outpost3, 30, outpost2);
            timeMachine.addEvent(launchEvent3);
        }

        // Creates a new game.
        public void newGame()
        {
            // Will do map generation here.
            state = new GameState();
            timeMachine = new TimeMachine(state);

        }

        // Moves the game to the next game tick.
        public void advanceTick()
        {
            // Should send a network request here before moving the player's game ahead.
            // New events can get added to the time machine before moving forward.

            // Advance the time machine
            timeMachine.goTo(state.goToNextTick());
            state = timeMachine.getState(); // Update the server's current game state.
        }

        // Moves the game to the next game tick.
        public void rewindTick()
        {
            // Should send a network request here before moving the player's game ahead.
            // New events can get added to the time machine before moving forward.

            // Advance the time machine
            timeMachine.goTo(state.getCurrentTick().getPreviousTick());
            state = timeMachine.getState(); // Update the server's current game state.
        }


        // THIS IS JUST FOR TESTING.
        // THE UPDATE LOOP CALLS THIS FUNCTION.
        // USING THIS TO ADVANCE THE GAME WHILE WE HAVE NO GUI.
        public void update()
        {
            if (state.currentTick.getTick() > 2000)
            {
                forward = false;
            }
            if (state.currentTick.getTick() == 108 && !addedEvents)
            {
                addedEvents = true;
                SubLaunchEvent launchEvent4 = new SubLaunchEvent(state.currentTick.advance(2), state.getOutposts()[state.getOutposts().Count - 1], 5, state.getSubList()[state.getSubList().Count - 1]);
                timeMachine.addEvent(launchEvent4);
                // Testing pirates
                /*
                SubLaunchEvent launchEvent10 = new SubLaunchEvent(state.currentTick.advance(1), state.getOutposts()[state.getOutposts().Count - 1], 1, state.getSubList()[state.getSubList().Count - 1]);
                SubLaunchEvent launchEvent12 = new SubLaunchEvent(state.currentTick.advance(100), state.getOutposts()[state.getOutposts().Count - 1], 1, state.getSubList()[state.getSubList().Count - 1]);
                SubLaunchEvent launchEvent11 = new SubLaunchEvent(state.currentTick.advance(600), state.getOutposts()[state.getOutposts().Count - 1], 1, state.getSubList()[state.getSubList().Count - 1]);
                SubLaunchEvent launchEvent13 = new SubLaunchEvent(state.currentTick.advance(1200), state.getOutposts()[state.getOutposts().Count - 1], 1, state.getSubList()[state.getSubList().Count - 1]);
                SubLaunchEvent launchEvent14 = new SubLaunchEvent(state.currentTick.advance(1800), state.getOutposts()[state.getOutposts().Count - 1], 1, state.getSubList()[state.getSubList().Count - 1]);
                SubLaunchEvent launchEvent22 = new SubLaunchEvent(state.currentTick.advance(2400), state.getOutposts()[state.getOutposts().Count - 1], 1, state.getSubList()[state.getSubList().Count - 1]);
                SubLaunchEvent launchEvent23 = new SubLaunchEvent(state.currentTick.advance(3000), state.getOutposts()[state.getOutposts().Count - 1], 1, state.getSubList()[state.getSubList().Count - 1]);
                SubLaunchEvent launchEvent24 = new SubLaunchEvent(state.currentTick.advance(3600), state.getOutposts()[state.getOutposts().Count - 1], 1, state.getSubList()[state.getSubList().Count - 1]);

                timeMachine.addEvent(launchEvent10);
                timeMachine.addEvent(launchEvent11);
                timeMachine.addEvent(launchEvent12);
                timeMachine.addEvent(launchEvent13);
                timeMachine.addEvent(launchEvent14);
                timeMachine.addEvent(launchEvent22);
                timeMachine.addEvent(launchEvent23);
                timeMachine.addEvent(launchEvent24);

                SubLaunchEvent launchEvent15 = new SubLaunchEvent(state.currentTick.advance(300), state.getOutposts()[state.getOutposts().Count - 3], 1, state.getSubList()[state.getSubList().Count - 1]);
                SubLaunchEvent launchEvent16 = new SubLaunchEvent(state.currentTick.advance(900), state.getOutposts()[state.getOutposts().Count - 3], 1, state.getSubList()[state.getSubList().Count - 1]);
                SubLaunchEvent launchEvent17 = new SubLaunchEvent(state.currentTick.advance(1500), state.getOutposts()[state.getOutposts().Count - 3], 1, state.getSubList()[state.getSubList().Count - 1]);
                SubLaunchEvent launchEvent18 = new SubLaunchEvent(state.currentTick.advance(2100), state.getOutposts()[state.getOutposts().Count - 3], 1, state.getSubList()[state.getSubList().Count - 1]);
                SubLaunchEvent launchEvent19 = new SubLaunchEvent(state.currentTick.advance(2700), state.getOutposts()[state.getOutposts().Count - 3], 1, state.getSubList()[state.getSubList().Count - 1]);
                SubLaunchEvent launchEvent20 = new SubLaunchEvent(state.currentTick.advance(3300), state.getOutposts()[state.getOutposts().Count - 3], 1, state.getSubList()[state.getSubList().Count - 1]); 
                SubLaunchEvent launchEvent21 = new SubLaunchEvent(state.currentTick.advance(3900), state.getOutposts()[state.getOutposts().Count - 3], 1, state.getSubList()[state.getSubList().Count - 1]);
                timeMachine.addEvent(launchEvent15);
                timeMachine.addEvent(launchEvent16);
                timeMachine.addEvent(launchEvent17);
                timeMachine.addEvent(launchEvent18);
                timeMachine.addEvent(launchEvent19);
                timeMachine.addEvent(launchEvent20);
                timeMachine.addEvent(launchEvent21);
                */
            }
            if (state.currentTick.getTick() == 0)
            {
                forward = true;
            }
            if (forward)
            {
                advanceTick();
            }
            else
            {
                rewindTick();
            }
        }
    }
}
