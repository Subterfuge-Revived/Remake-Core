using System;
using System.Collections.Generic;
using System.Linq;
using SubterfugeCore.Core.Entities.Specialists.Effects;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public class SpecialistPool
    {
        /// <summary>
        /// Holds a collection of all of the specialists being used in the active game.
        /// </summary>
        private readonly Dictionary<string, SpecialistConfiguration> _specialistPool = new Dictionary<string, SpecialistConfiguration>();
        private readonly List<string> _specialistHireOrder = new List<string>();

        public SpecialistPool(SeededRandom seededRandom, List<SpecialistConfiguration> allowedSpecialists)
        {
            foreach (SpecialistConfiguration specConfig in allowedSpecialists)
            {
                AddSpecialistToPool(specConfig);
            }
            
            // Create the specialist hire order.
            // This algorithm will create 3 seperate lists of the allowed specialists.
            // The lists are then randomized in place, to ensure one full cycle of all allowed specialists is offered before a second is offered.
            // All three lists are appended together.
            // This results in 3 specialists of each type in the pool while preventing two of the same specialist getting offered before another player has seen it once.
            _specialistHireOrder
                .AddRange(_specialistPool.Keys.ToList().OrderBy(it => seededRandom.NextRand(0, 1000)));
            _specialistHireOrder
                .AddRange(_specialistPool.Keys.ToList().OrderBy(it => seededRandom.NextRand(0, 1000)));
            _specialistHireOrder
                .AddRange(_specialistPool.Keys.ToList().OrderBy(it => seededRandom.NextRand(0, 1000)));
        }

        /// <summary>
        /// Loads a specialist into the specialist pool for this game.
        /// </summary>
        /// <param name="configuration">The specialist configuration object to add to the pool</param>
        /// <returns>If the configuration could be added to the pool</returns>
        /// <exception cref="Exception">Throws an exception of a specialist with that name already exists in the game</exception>
        private bool AddSpecialistToPool(SpecialistConfiguration configuration)
        {
            if (_specialistPool.ContainsKey(configuration.SpecialistName))
            {
                throw new Exception("Specialist name already exists.");
            }
            _specialistPool.Add(configuration.SpecialistName, configuration);
            return true;
        }

        public List<SpecialistConfiguration> GetSpecialistsAvailableForHire(int numberToOffer)
        {
            var offeredSpecialistNames = _specialistHireOrder.Take(numberToOffer).ToList();
            _specialistHireOrder.RemoveRange(0, numberToOffer);
            _specialistHireOrder.AddRange(offeredSpecialistNames);
            return offeredSpecialistNames.Select(specName => _specialistPool[specName]).ToList();
        }

        public Specialist SpawnSpecialist(string specialistName, Player player)
        {
            if (_specialistPool.ContainsKey(specialistName))
            {                
                SpecialistConfiguration configuration = _specialistPool[specialistName];
                Specialist spawnedSpecialist = new Specialist(specialistName, (int)configuration.Priority, player);
                
                // Create the specialist effects.
                SpecialistEffectFactory effectFactory = new SpecialistEffectFactory();
                foreach (var effectConfiguration in configuration.SpecialistEffects.ToList())
                {
                    ISpecialistEffect effect = effectFactory.CreateSpecialistEffect(effectConfiguration);
                    spawnedSpecialist.AddSpecialistEffect(effect);
                }

                return spawnedSpecialist;
            }
            return null;
        }
    }
}