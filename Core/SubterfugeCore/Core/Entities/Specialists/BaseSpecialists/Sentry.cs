using SubterfugeCore.Core.Players;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public class Sentry : Specialist
    {
        public Sentry(Player owner) : base(owner, new SpecialistConfiguration()
        {
            Id = "Sentry",
            Priority = 1,
            SpecialistName = "Sentry",
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
                        Value = 5,
                        ValueType = ValueType.Numeric,
                    },
                    EffectTrigger = EffectTrigger.AoeTick,
                    EffectTarget = EffectTarget.Enemy,
                    EffectModifier = EffectModifier.Driller,
                    CooldownTicks = 10,
                    Target = AoeTarget.Closest,
                },
            },
            SpecialistClasses = { SpecialistClass.Zoner },
            PromotesFrom = null,
            OnlyActiveIfCaptured = false,
            NumberOnHire = 1,
        })
        {
        }
    }
}