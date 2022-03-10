using SubterfugeCore.Core.Players;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public class Inspector : Specialist
    {
        public Inspector(Player owner) : base(owner, new SpecialistConfiguration()
        {
            Id = "Inspector",
            Priority = 1,
            SpecialistName = "Inspector",
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
                        Value = 1.0f,
                        ValueType = ValueType.Percentage,
                    },
                    EffectTrigger = EffectTrigger.CombatVictory,
                    EffectTarget = EffectTarget.Friendly,
                    EffectModifier = EffectModifier.ShieldValue,
                    CooldownTicks = 0,
                },
                new SpecialistEffectConfiguration
                {
                    Value = new EffectValue()
                    {
                        Value = 1.0f,
                        ValueType = ValueType.Percentage,
                    },
                    EffectTrigger = EffectTrigger.ArriveOutpost,
                    EffectTarget = EffectTarget.Friendly,
                    EffectModifier = EffectModifier.ShieldValue,
                    CooldownTicks = 0,
                },
            },
            SpecialistClasses = { SpecialistClass.Protector },
            PromotesFrom = null,
            OnlyActiveIfCaptured = false,
            NumberOnHire = 1,
        })
        {
        }
    }
}