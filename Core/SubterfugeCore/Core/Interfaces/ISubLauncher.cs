using GameEventModels;
using SubterfugeCore.Core.GameEvents;

namespace SubterfugeCore.Core.Interfaces
{
    /// <summary>
    /// Anything that can launch subs from its location
    /// </summary>
    public interface ISubLauncher : IPosition, IOwnable
    {
        /// <summary>
        /// Launches a sub from the location
        /// </summary>
        /// <param name="drillerCount">The number of drillers to send</param>
        /// <param name="destination">The sub's destination</param>
        /// <returns>The launched sub</returns>
        ICombatable LaunchSub(GameState state, LaunchEvent launchEventData);
        
        /// <summary>
        /// Undoes a sub launch
        /// </summary>
        /// <param name="sub">The sub to undo launching</param>
        void UndoLaunch(GameState state, LaunchEvent launchEventData);
    }
}
