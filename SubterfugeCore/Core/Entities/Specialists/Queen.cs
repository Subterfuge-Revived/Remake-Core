using SubterfugeCore.Core.Entities.Specialists.Effects;
using SubterfugeCore.Core.Players;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public class Queen : Specialist
    {
        public Queen(Player owner) : base("Queen", 0, owner)
        {
            // Create up to X 'SpecialistHireAvaliable' Events
        }

        public override string getEffectAsText(ISpecialistEffect effect)
        {
            return null;
        }
    }
}