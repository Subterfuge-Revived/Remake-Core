using System;
using System.Collections.Generic;
using SubterfugeCore.Core.Entities.Specialists.Effects;
using SubterfugeCore.Core.Players;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public class SpecialistPool
    {
        /// <summary>
        /// Holds a collection of all of the specialists being used in the active game.
        /// </summary>
        private readonly Dictionary<string, SpecialistConfiguration> _specialistPool = new Dictionary<string, SpecialistConfiguration>();

        /// <summary>
        /// Loads a specialist into the specialist pool for this game.
        /// </summary>
        /// <param name="configuration">The specialist configuration object to add to the pool</param>
        /// <returns>If the configuration could be added to the pool</returns>
        /// <exception cref="Exception">Throws an exception of a specialist with that name already exists in the game</exception>
        public bool AddSpecialistToPool(SpecialistConfiguration configuration)
        {
            if (_specialistPool.ContainsKey(configuration.SpecialistName))
            {
                throw new Exception("Specialist name already exists.");
            }
            _specialistPool.Add(configuration.SpecialistName, configuration);
            return true;
        }

        public Specialist SpawnSpecialist(string specialistName, Player player)
        {
            if (_specialistPool.ContainsKey(specialistName))
            {                
                SpecialistConfiguration configuration = _specialistPool[specialistName];
                Specialist spawnedSpecialist = new Specialist(specialistName, configuration.Priority, player);
                
                // Create the specialist effects.
                SpecialistEffectFactory effectFactory = new SpecialistEffectFactory();
                foreach (SpecialistEffectConfiguration effectConfiguration in configuration.SpecialistEffectConfigurations)
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