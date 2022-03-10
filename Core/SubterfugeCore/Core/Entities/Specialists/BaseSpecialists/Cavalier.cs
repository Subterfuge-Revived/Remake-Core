using SubterfugeCore.Core.Players;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public class InfiltratorTwo : Specialist
    {
        public InfiltratorTwo(Player owner) : base(owner, new SpecialistConfiguration()
        {
            Id = "Cavalier",
            Priority = 1,
            SpecialistName = "Cavalier",
            Creator = new User()
            {
                Id = "0",
                Username = "Subterfuge"
            },
            SpecialistEffects =
            {
                // Destroy 25 enemy shields on combat.
                new SpecialistEffectConfiguration
                {
                    Value = new EffectValue()
                    {
                        Value = 25.0f,
                        ValueType = ValueType.Numeric,
                    },
                    EffectTrigger = EffectTrigger.PreCombat,
                    EffectTarget = EffectTarget.Enemy,
                    EffectModifier = EffectModifier.ShieldValue,
                    CooldownTicks = 0,
                },
                // After a combat win, restore 50% of the shield capacity
                new SpecialistEffectConfiguration
                {
                    Value = new EffectValue()
                    {
                        Value = 0.50f,
                        ValueType = ValueType.Percentage,
                    },
                    EffectTrigger = EffectTrigger.CombatVictory,
                    EffectTarget = EffectTarget.Friendly,
                    EffectModifier = EffectModifier.ShieldMaxValue,
                    CooldownTicks = 0,
                },
            },
            SpecialistClasses = { SpecialistClass.Piercer, SpecialistClass.Warrior },
            PromotesFrom = "Infiltrator",
            OnlyActiveIfCaptured = false,
        })
        {
        }
    }
}