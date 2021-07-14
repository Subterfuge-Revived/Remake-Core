using System;
using System.Collections.Generic;
using System.Numerics;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Core.Topologies;

namespace SubterfugeCore.Core.Interfaces
{
    /// <summary>
    /// Anything that can perform combat
    /// </summary>
    public interface ICombatable : IDrillerCarrier, ISpecialistCarrier, ITargetable, IShieldable, ISubLauncher
    {
        //Consider making this an abstract class as the implementation in Outpost and Sub is identical

        /// <summary>
        /// Gets whether the given player have visibility of this Combatable.
        /// </summary>
        /// <param name="player">The player to check visibility</param>
        /// <returns>True if the Combatable is visible, and false otherwise.</returns>
        bool IsVisibleTo(Player player);

        /// <summary>
        /// Gets a list of all players that have visibility of this Combatable.
        /// </summary>
        /// <returns>A list of all players that have visibility of this Combatable.</returns>
        List<Player> GetVisibleTo();

        /// <summary>
        /// Adds or removes visibility from a player to this Combatable.
        /// </summary>
        /// <param name="player">The player to alter visibility.</param>
        /// <param name="visible">Whether the player should have visibility to this Combatable; true if the player should, and false otherwise.</param>
        void SetVisibleTo(Player player, bool visible);
    }
}