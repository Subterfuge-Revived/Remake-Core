using SubterfugeCore.Core.Players;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public class Ninja : Specialist
    {
        public Ninja(Player owner) : base(owner, new SpecialistConfiguration()
        {
            Id = "Ninja",
            Priority = 1,
            SpecialistName = "Ninja",
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
                        Value = 10f,
                        ValueType = ValueType.Numeric,
                    },
                    EffectTrigger = EffectTrigger.ManualTrigger,
                    EffectTarget = EffectTarget.Friendly,
                    EffectModifier = EffectModifier.Invisibility,
                    CooldownTicks = 0,
                },
            },
            SpecialistClasses = { SpecialistClass.Thief, SpecialistClass.Warrior },
            PromotesFrom = null,
            OnlyActiveIfCaptured = false,
            NumberOnHire = 1,
        })
        {
        }
    }
}