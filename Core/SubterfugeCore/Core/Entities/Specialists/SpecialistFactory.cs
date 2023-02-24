using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Core.Entities.Specialists
{
    public class SpecialistFactory
    {
        public static Specialist CreateSpecialist(SpecialistIds specialistId, Player owner)
        {
            switch (specialistId)
            {
                default:
                    return null;
            }
        }
    }
}