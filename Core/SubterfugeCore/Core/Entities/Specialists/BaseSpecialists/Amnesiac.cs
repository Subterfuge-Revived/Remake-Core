using SubterfugeCore.Core.Players;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public class Amnesiac : Specialist
    {
        public Amnesiac(Player owner) : base(owner, new SpecialistConfiguration()
        {
            Id = "Amnesiac",
            Priority = 1,
            SpecialistName = "Amnesiac",
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
                        Value = 1,
                        ValueType = ValueType.Numeric,
                    },
                    EffectTrigger = EffectTrigger.PreCombat,
                    EffectTarget = EffectTarget.Enemy,
                    EffectModifier = EffectModifier.DemoteSpecialists,
                    CooldownTicks = 0,
                },
                new SpecialistEffectConfiguration
                {
                    Value = new EffectValue()
                    {
                        Value = 1,
                        ValueType = ValueType.Numeric,
                    },
                    EffectTrigger = EffectTrigger.PostCombat,
                    EffectTarget = EffectTarget.Enemy,
                    EffectModifier = EffectModifier.PromoteSpecialists,
                    CooldownTicks = 0,
                },
            },
            SpecialistClasses = { SpecialistClass.Influencer },
            PromotesFrom = null,
            OnlyActiveIfCaptured = false,
            NumberOnHire = 1,
        })
        {
        }
    }
}