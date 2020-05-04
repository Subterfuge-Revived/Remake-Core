using System.Collections.Generic;
using SubterfugeCore.Core.Entities.Specialists.Effects;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public class SpecialistConfiguration
    {
        /// <summary>
        /// The specialist priority
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// The name of the specialist
        /// </summary>
        public string SpecialistName { get; set; }
        
        /// <summary>
        /// A list of specialist effects that the specialist can apply
        /// </summary>
        public List<SpecialistEffectConfiguration> SpecialistEffectConfigurations { get; } = new List<SpecialistEffectConfiguration>();
    }
}