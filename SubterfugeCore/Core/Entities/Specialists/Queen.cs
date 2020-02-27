using SubterfugeCore.Core.Entities.Specialists.Effects;
using SubterfugeCore.Core.Players;

namespace SubterfugeCore.Core.Entities.Specialists
{
    /// <summary>
    /// The queen specialist.
    /// </summary>
    public class Queen : Specialist
    {
        /// <summary>
        /// Creates an instance of a queen belonging to the player
        /// </summary>
        /// <param name="owner">The owner of the queen</param>
        public Queen(Player owner) : base("Queen", 0, owner)
        {
            // Create up to X 'SpecialistHireAvaliable' Events
        }

        /// <summary>
        /// Returns nothing.
        /// </summary>
        /// <param name="effect">The effect to get text of</param>
        /// <returns>Nothing</returns>
        public override string getEffectAsText(ISpecialistEffect effect)
        {
            return null;
        }
    }
}