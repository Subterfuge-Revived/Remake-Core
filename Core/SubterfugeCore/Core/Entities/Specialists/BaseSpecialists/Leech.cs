using SubterfugeCore.Core.Players;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public class Leech : Specialist
    {
        public Leech(Player owner) : base(owner, new SpecialistConfiguration()
        {
            Id = "Leech",
            Priority = 1,
            SpecialistName = "Leech",
            Creator = new User()
            {
                Id = "0",
                Username = "Subterfuge"
            },
            SpecialistEffects =
            {
                // Decrease shield regeneration by 25%
                new SpecialistEffectConfiguration
                {
                    Value = new EffectValue()
                    {
                        Value = -0.25f,
                        ValueType = ValueType.Percentage,
                    },
                    EffectTrigger = EffectTrigger.ArriveOutpost,
                    EffectTarget = EffectTarget.Enemy,
                    EffectModifier = EffectModifier.ShieldRegeneration,
                    CooldownTicks = 0,
                },
                // After a combat win, restore 50% of the shield capacity
                new SpecialistEffectConfiguration
                {
                    Value = new EffectValue()
                    {
                        Value = 1,
                        ValueType = ValueType.Numeric,
                    },
                    EffectTrigger = EffectTrigger.CombatLoss,
                    EffectTarget = EffectTarget.Friendly,
                    EffectModifier = EffectModifier.CannotBeReleased,
                    CooldownTicks = 0,
                },
            },
            SpecialistClasses = { SpecialistClass.Leecher, SpecialistClass.Thief },
            PromotesFrom = null,
            OnlyActiveIfCaptured = true,
        })
        {
        }
    }
}