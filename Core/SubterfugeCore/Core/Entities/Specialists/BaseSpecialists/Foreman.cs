using SubterfugeCore.Core.Players;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public class Foreman : Specialist
    {
        public Foreman(Player owner) : base(owner, new SpecialistConfiguration()
        {
            Id = "Foreman",
            Priority = 1,
            SpecialistName = "Foreman",
            Creator = new User()
            {
                Id = "0",
                Username = "Subterfuge"
            },
            SpecialistEffects =
            {
                new SpecialistEffectConfiguration
                {
                    Value = new EffectValue()
                    {
                        Value = 4,
                        ValueType = ValueType.Numeric,
                    },
                    EffectTrigger = EffectTrigger.FactoryProduce,
                    EffectTarget = EffectTarget.Friendly,
                    EffectModifier = EffectModifier.Driller,
                    CooldownTicks = 0,
                },
            },
            SpecialistClasses = { SpecialistClass.Producer },
            PromotesFrom = null,
            OnlyActiveIfCaptured = false,
            NumberOnHire = 1,
        })
        {
        }
    }
}