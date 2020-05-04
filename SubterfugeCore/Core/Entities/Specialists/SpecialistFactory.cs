using System;
using System.Collections.Generic;
using SubterfugeCore.Core.Entities.Specialists.Effects;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public class SpecialistFactory
    {
        /// <summary>
        /// Holds a collection of all of the specialists being used in the active game.
        /// </summary>
        public static Dictionary<string, SpecialistConfiguration> specialistPool = new Dictionary<string, SpecialistConfiguration>();

        public SpecialistFactory()
        {
            
        }

        /// <summary>
        /// Loads a specialist into the specialist pool for this game.
        /// </summary>
        /// <param name="configuration">The specialist configuration object to add to the pool</param>
        /// <returns>If the configuration could be added to the pool</returns>
        /// <exception cref="Exception">Throws an exception of a specialist with that name already exists in the game</exception>
        public static bool LoadSpecialist(SpecialistConfiguration configuration)
        {
            if (specialistPool.ContainsKey(configuration.SpecialistName))
            {
                throw new Exception("Specialist name already exists.");
                return false;
            }
            specialistPool.Add(configuration.SpecialistName, configuration);
            return true;
        }

        public static Specialist SpawnSpecialist(string specialistName)
        {
            if (specialistPool.ContainsKey(specialistName))
            {
                SpecialistConfiguration configuration = specialistPool[specialistName];
                
                // Create the specialist effects.
                SpecialistEffectFactory effectFactory = new SpecialistEffectFactory();

                foreach (SpecialistEffectConfiguration effectConfiguration in configuration.SpecialistEffectConfigurations)
                {
                    ISpecialistEffect effect = effectFactory.createSpecialistEffect(effectConfiguration);
                    // TODO: Add the specialist effects to the spawned specialist.
                }
                
                // TODO: Return the spawned specialist.
                return null;
            }
            else
            {
                return null;
            }
        }
    }
}