using System;
using SubterfugeCore.Core.Entities;

namespace SubterfugeCore.Core.Interfaces
{
    /// <summary>
    /// Anything that can launch subs from its location
    /// </summary>
    public interface ILaunchable : IPosition, IOwnable
    {
        /// <summary>
        /// Launches a sub from the location
        /// </summary>
        /// <param name="drillerCount">The number of drillers to send</param>
        /// <param name="destination">The sub's destination</param>
        /// <returns>The launched sub</returns>
        Sub LaunchSub(int drillerCount, ITargetable destination);
        
        /// <summary>
        /// Undoes a sub launch
        /// </summary>
        /// <param name="sub">The sub to undo launching</param>
        void UndoLaunch(Sub sub);
    }
}
