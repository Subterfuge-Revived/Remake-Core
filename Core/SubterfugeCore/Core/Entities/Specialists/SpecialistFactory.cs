using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Specialists.Specialists;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Core.Entities.Specialists
{
    public class SpecialistFactory
    {
        public static Specialist CreateSpecialist(SpecialistTypeId specialistTypeId, Player owner)
        {
            switch (specialistTypeId)
            {
                /*case SpecialistTypeId.Queen:
                    return new Queen(owner);*/
                case SpecialistTypeId.Advisor:
                    return new Advisor(owner);
                /*case SpecialistTypeId.Foreman:
                    return new Foreman(owner);
                case SpecialistTypeId.Helmsman:
                    return new Helmsman(owner);
                case SpecialistTypeId.Infiltrator:
                    return new Infiltrator(owner);
                case SpecialistTypeId.Smuggler:
                    return new Smuggler(owner);
                case SpecialistTypeId.Veteran:
                    return new Veteran(owner);*/
                default:
                    return null;
            }
        }
    }
}