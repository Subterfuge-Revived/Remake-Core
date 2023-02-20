using System;
using System.Collections.Generic;
using System.Linq;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Config;
using Subterfuge.Remake.Core.Entities.Specialists.Effects;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.GameEvents.PlayerTriggeredEvents;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities.Specialists
{
    public class SpecialistPool
    {
        /// <summary>
        /// Holds a collection of all of the specialists being used in the active game.
        /// </summary>
        private List<SpecialistConfiguration> _specialistHireOrder = new List<SpecialistConfiguration>();
        private List<SpecialistConfiguration> _hiredSpecialists = new List<SpecialistConfiguration>();
        private int currentHireIndex = 0;

        private Player _player;

        public SpecialistPool(
            Player player,
            SeededRandom seededRandom,
            List<string> playerSpecialistDeck
        )
        {
            _player = player;
            var globalSpecialistPool = Game.SpecialistConfigurationPool.GetPool();

            var playerPool = playerSpecialistDeck
                .Select(specialistId =>
                    globalSpecialistPool.FirstOrDefault(specialistConfig => specialistConfig.Id == specialistId))
                .Where(it => it != null)
                .ToList();

            // Create the specialist hire order.
            // Create N lists where N is the number of offered specialists. This ensures that the elements in the list are divisible by N
            for (int i = 0; i < Constants.SPECIALISTS_OFFERED_PER_CYCLE; i++)
            {
                _specialistHireOrder
                    .AddRange(playerPool.OrderBy(it => seededRandom.NextRand(0, 1000)));
            }
        }

        /// <summary>
        /// Look at the N next specialists in the pool
        /// </summary>
        /// <returns></returns>
        public List<SpecialistConfiguration> PeekOffers()
        {
            var index = (currentHireIndex * Constants.SPECIALISTS_OFFERED_PER_CYCLE) % _specialistHireOrder.Count;
            return _specialistHireOrder.GetRange(index, Constants.SPECIALISTS_OFFERED_PER_CYCLE);
        }

        public Specialist HireSpecialist(SpecialistConfiguration specialistConfiguration) {
            if (PeekOffers().Contains(specialistConfiguration))
            {
                currentHireIndex++;
                _hiredSpecialists.Add(specialistConfiguration);
                return new Specialist(specialistConfiguration, _player);
            }
            return null;
        }

        public void UndoSpecialistHire()
        {
            currentHireIndex--;
        }
    }
}