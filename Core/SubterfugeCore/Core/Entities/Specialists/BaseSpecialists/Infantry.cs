using SubterfugeCore.Core.Players;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public class Infantry : Specialist
    {
        public Infantry(Player owner) : base(owner, new SpecialistConfiguration()
        {
            Id = "Infantry",
            Priority = 1,
            SpecialistName = "Infantry",
            Creator = new User()
            {
                Id = "0",
                Username = "Subterfuge"
            },
            SpecialistEffects =
            {
                // Consume for 15 drillers
                new SpecialistEffectConfiguration
                {
                    Value = new EffectValue()
                    {
                        Value = 15,
                        ValueType = ValueType.Numeric,
                    },
                    EffectTrigger = EffectTrigger.ManualTrigger,
                    EffectTarget = EffectTarget.Friendly,
                    EffectModifier = EffectModifier.Driller,
                    CooldownTicks = 0,
                },
                new SpecialistEffectConfiguration
                {
                    Value = new EffectValue()
                    {
                        Value = 1,
                        ValueType = ValueType.Numeric,
                    },
                    EffectTrigger = EffectTrigger.ManualTrigger,
                    EffectTarget = EffectTarget.Friendly,
                    EffectModifier = EffectModifier.KillThisSpecialist,
                    CooldownTicks = 0,
                },
            },
            SpecialistClasses = { SpecialistClass.Controller },
            PromotesFrom = null,
            OnlyActiveIfCaptured = false,
            NumberOnHire = 2,
        })
        {
        }
    }
}