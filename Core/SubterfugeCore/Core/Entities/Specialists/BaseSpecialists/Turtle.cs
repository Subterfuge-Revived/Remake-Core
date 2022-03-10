using SubterfugeCore.Core.Players;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public class Turtle : Specialist
    {
        public Turtle(Player owner) : base(owner, new SpecialistConfiguration()
        {
            Id = "Turtle",
            Priority = 1,
            SpecialistName = "Turtle",
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
                        Value = -10,
                        ValueType = ValueType.Numeric,
                    },
                    EffectTrigger = EffectTrigger.SpendCurrency,
                    EffectTarget = EffectTarget.Friendly,
                    EffectModifier = EffectModifier.Driller,
                    CooldownTicks = 0,
                },
                new SpecialistEffectConfiguration
                {
                    Value = new EffectValue()
                    {
                        Value = 0.10f,
                        ValueType = ValueType.Percentage,
                    },
                    EffectTrigger = EffectTrigger.CurrencyGain,
                    EffectTarget = EffectTarget.Friendly,
                    EffectModifier = EffectModifier.CurrencyValue,
                    CooldownTicks = 0,
                },
            },
            SpecialistClasses = { SpecialistClass.Investor, },
            PromotesFrom = null,
            OnlyActiveIfCaptured = false,
            NumberOnHire = 1,
        })
        {
        }
    }
}