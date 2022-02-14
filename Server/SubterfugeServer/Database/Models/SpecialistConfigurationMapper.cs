using System.Collections.Generic;
using System.Linq;
using SubterfugeRemakeService;

namespace SubterfugeServerConsole.Connections
{
    public class SpecialistConfigurationMapper : ProtoClassMapper<SpecialistConfiguration>
    {
        public string Id;
        public long Priority;
        public string SpecialistName;
        public User Creator;
        public List<SpecialistEffectConfiguration> SpecialistEffects;
        public string PromotesFrom;

        public SpecialistConfigurationMapper(SpecialistConfiguration configuration)
        {
            Id = configuration.Id;
            Priority = configuration.Priority;
            SpecialistName = configuration.SpecialistName;
            Creator = configuration.Creator;
            SpecialistEffects = configuration.SpecialistEffects.ToList();
            PromotesFrom = configuration.PromotesFrom;
        }
        
        public override SpecialistConfiguration ToProto()
        {
            return new SpecialistConfiguration()
            {
                Id = Id,
                Priority = Priority,
                SpecialistName = SpecialistName,
                Creator = Creator,
                SpecialistEffects = { SpecialistEffects },
                PromotesFrom = PromotesFrom
            };
        }
    }
}