using System;
using System.Collections.Generic;
using System.Linq;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities.Specialists
{
    public class SpecialistPool
    {
        /// <summary>
        /// Holds a collection of all of the specialists being used in the active game.
        /// </summary>
        private List<SpecialistTypeId> _specialistHireOrder = new List<SpecialistTypeId>();
        private List<SpecialistTypeId> _hiredSpecialists = new List<SpecialistTypeId>();
        private int currentHireIndex = 0;

        private Player _player;

        public SpecialistPool(
            Player player,
            SeededRandom seededRandom,
            List<SpecialistTypeId> playerSpecialistDeck
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
        public List<SpecialistTypeId> PeekOffers()
        {
            var index = (currentHireIndex * Constants.SPECIALISTS_OFFERED_PER_CYCLE) % _specialistHireOrder.Count;
            return _specialistHireOrder.GetRange(index, Constants.SPECIALISTS_OFFERED_PER_CYCLE);
        }

        public Specialist HireSpecialist(SpecialistTypeId specialistTypeId) {
            if (PeekOffers().Contains(specialistTypeId))
            {
                currentHireIndex++;
                _hiredSpecialists.Add(specialistTypeId);
                return SpecialistFactory.CreateSpecialist(specialistTypeId, _player.SpecialistPool.currentHireIndex, _player);
            }
            return null;
        }

        public void UndoSpecialistHire()
        {
            currentHireIndex--;
        }
    }
}