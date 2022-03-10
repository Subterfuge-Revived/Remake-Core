using SubterfugeCore.Core.Players;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public class Convict : Specialist
    {
        public Convict(Player owner) : base(owner, new SpecialistConfiguration()
        {
            Id = "Convict",
            Priority = 1,
            SpecialistName = "Convict",
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
                    EffectTrigger = EffectTrigger.LocalTick,
                    EffectTarget = EffectTarget.Enemy,
                    EffectModifier = EffectModifier.Driller,
                    CooldownTicks = 5,
                },
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
            SpecialistClasses = { SpecialistClass.Leecher },
            PromotesFrom = null,
            OnlyActiveIfCaptured = true,
            NumberOnHire = 1,
        })
        {
        }
    }
}