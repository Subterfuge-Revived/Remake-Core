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
        public static Dictionary<string, SpecialistConfiguration> specialistPool = new Dictionary<string, SpecialistConfiguration>();

        public SpecialistPool()
        {
            
        }

        /// <summary>
        /// Loads a specialist into the specialist pool for this game.
        /// </summary>
        /// <param name="configuration">The specialist configuration object to add to the pool</param>
        /// <returns>If the configuration could be added to the pool</returns>
        /// <exception cref="Exception">Throws an exception of a specialist with that name already exists in the game</exception>
        public bool AddSpecialistToPool(SpecialistConfiguration configuration)
        {
            if (specialistPool.ContainsKey(configuration.SpecialistName))
            {
                throw new Exception("Specialist name already exists.");
                return false;
            }
            specialistPool.Add(configuration.SpecialistName, configuration);
            return true;
        }

        public Specialist SpawnSpecialist(string specialistName, Player player)
        {
            if (specialistPool.ContainsKey(specialistName))
            {                
                SpecialistConfiguration configuration = specialistPool[specialistName];
                
                Specialist spawnedSpecialist = new Specialist(specialistName, configuration.Priority, player);
                
                // Create the specialist effects.
                SpecialistEffectFactory effectFactory = new SpecialistEffectFactory();

                foreach (SpecialistEffectConfiguration effectConfiguration in configuration.SpecialistEffectConfigurations)
                {
                    ISpecialistEffect effect = effectFactory.createSpecialistEffect(effectConfiguration);
                    spawnedSpecialist.AddSpecialistEffect(effect);
                }

                return spawnedSpecialist;
            }
            else
            {
                return null;
            }
        }
    }
}