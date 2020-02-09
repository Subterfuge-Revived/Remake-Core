using SubterfugeCore.Components.Outpost;
using SubterfugeCore.Core.Interfaces.Outpost;
using SubterfugeCore.Entities;

namespace SubterfugeCore.Core.Components.Outpost
{
    /// <summary>
    /// Anything that can launch subs from its location
    /// </summary>
    public interface ILaunchable : ILocation
    {
        /// <summary>
        /// Launches a sub from the location
        /// </summary>
        /// <param name="drillerCount">The number of drillers to send</param>
        /// <param name="destination">The sub's destination</param>
        /// <returns>The launched sub</returns>
        Sub launchSub(int drillerCount, ITargetable destination);
        
        /// <summary>
        /// Undoes a sub launch
        /// </summary>
        /// <param name="sub">The sub to undo launching</param>
        void undoLaunch(Sub sub);
    }
}
