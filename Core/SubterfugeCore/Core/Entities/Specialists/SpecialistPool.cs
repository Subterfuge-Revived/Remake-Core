using System;
using System.Collections.Generic;
using System.Linq;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Config;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities.Specialists
{
    public class SpecialistPool
    {
        /// <summary>
        /// Holds a collection of all of the specialists being used in the active game.
        /// </summary>
        private List<SpecialistIds> _specialistHireOrder = new List<SpecialistIds>();
        private List<SpecialistIds> _hiredSpecialists = new List<SpecialistIds>();
        private int currentHireIndex = 0;

        private Player _player;

        public SpecialistPool(
            Player player,
            SeededRandom seededRandom,
            List<SpecialistIds> playerSpecialistDeck
        )
        {
            _player = player;

            // Create the specialist hire order.
            // Create N lists where N is the number of offered specialists. This ensures that the elements in the list are divisible by N
            for (int i = 0; i < Constants.SPECIALISTS_OFFERED_PER_CYCLE; i++)
            {
                _specialistHireOrder
                    .AddRange(playerSpecialistDeck.OrderBy(it => seededRandom.NextRand(0, 1000)));
            }
        }

        /// <summary>
        /// Look at the N next specialists in the pool
        /// </summary>
        /// <returns></returns>
        public List<SpecialistIds> PeekOffers()
        {
            var index = (currentHireIndex * Constants.SPECIALISTS_OFFERED_PER_CYCLE) % _specialistHireOrder.Count;
            return _specialistHireOrder.GetRange(index, Constants.SPECIALISTS_OFFERED_PER_CYCLE);
        }

        public Specialist HireSpecialist(SpecialistIds specialistId) {
            if (PeekOffers().Contains(specialistId))
            {
                currentHireIndex++;
                _hiredSpecialists.Add(specialistId);
                return SpecialistFactory.CreateSpecialist(specialistId, _player);
            }
            return null;
        }

        public void UndoSpecialistHire()
        {
            currentHireIndex--;
        }
    }
}