using SubterfugeCore.Core.Players;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public class Thief : Specialist
    {
        public Thief(Player owner) : base(owner, new SpecialistConfiguration()
        {
            Id = "Thief",
            Priority = 1,
            SpecialistName = "Thief",
            Creator = new User()
            {
                Id = "0",
                Username = "Subterfuge"
            },
            SpecialistEffects =
            {
                // Steal 10% of drillers
                new SpecialistEffectConfiguration
                {
                    Value = new EffectValue()
                    {
                        Value = 0.10f,
                        ValueType = ValueType.Percentage,
                    },
                    EffectTrigger = EffectTrigger.PreCombat,
                    EffectTarget = EffectTarget.Enemy,
                    EffectModifier = EffectModifier.StealDrillers,
                    CooldownTicks = 0,
                },
            },
            SpecialistClasses = { SpecialistClass.Thief, SpecialistClass.Piercer },
            PromotesFrom = null,
            OnlyActiveIfCaptured = false,
        })
        {
        }
    }
}