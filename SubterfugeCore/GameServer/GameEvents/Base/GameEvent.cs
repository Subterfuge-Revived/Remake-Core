using SubterfugeCore.Timing;

namespace SubterfugeCore.GameEvents
{
    public abstract class GameEvent
    {
        private GameEventType commandType;
        private GameTick gameTick;

        /// <summary>
        /// This function will be executed when determing the game's state for the time machine.
        /// This function will check all conditions required to perform the command as well as perform the command
        /// to show the outcome of the command.
        /// </summary>
        public abstract void eventForwardAction();

        /// <summary>
        /// This function will be executed when going back in time in order to undo an action.
        /// For example, this will un-hire a specialist returning the hire point to the queen, or un-launch a sub returning the drillers to the owner.
        /// </summary>
        public abstract void eventBackwardAction();

        public abstract GameTick getTick();
    }
}
