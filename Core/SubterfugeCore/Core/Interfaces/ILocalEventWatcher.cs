using System.Collections.Generic;
using SubterfugeCore.Core.GameEvents.Base;

namespace SubterfugeCore.Core.Interfaces
{
    public interface ILocalEventWatcher
    {
        /// <summary>
        /// Gets the next game event that occurrs on this object
        /// </summary>
        /// <returns>The next game event to occur</returns>
        GameEvent GetNextEvent();
        
        /// <summary>
        /// Gets a list of all the game events that are sceduled to occur for this object.
        /// </summary>
        /// <returns></returns>
        List<GameEvent> GetEvents();
    }
}