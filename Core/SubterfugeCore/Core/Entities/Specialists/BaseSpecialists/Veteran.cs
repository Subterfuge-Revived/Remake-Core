using SubterfugeCore.Core.Players;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public class Veteran : Specialist
    {
        public Veteran(Player owner) : base(owner, new SpecialistConfiguration()
        {
            Id = "Veteran",
            Priority = 1,
            SpecialistName = "Veteran",
            Creator = new User()
            {
                Id = "0",
                Username = "Subterfuge"
            },
            SpecialistEffects =
            {
                // Destroy 15 drillers on combat
                new SpecialistEffectConfiguration
                {
                    Value = new EffectValue()
                    {
                        Value = 15,
                        ValueType = ValueType.Numeric,
                    },
                    EffectTrigger = EffectTrigger.PreCombat,
                    EffectTarget = EffectTarget.Enemy,
                    EffectModifier = EffectModifier.Driller,
                    CooldownTicks = 0,
                },
            },
            SpecialistClasses = { SpecialistClass.Warrior },
            PromotesFrom = null,
            OnlyActiveIfCaptured = false,
        })
        {
        }
    }
}