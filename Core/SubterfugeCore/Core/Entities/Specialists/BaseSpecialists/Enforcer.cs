using SubterfugeCore.Core.Players;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public class Enforcer : Specialist
    {
        public Enforcer(Player owner) : base(owner, new SpecialistConfiguration()
        {
            Id = "Enforcer",
            Priority = 1,
            SpecialistName = "Enforcer",
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
                        Value = -0.20f,
                        ValueType = ValueType.Percentage,
                    },
                    EffectTrigger = EffectTrigger.ArriveOutpost,
                    EffectTarget = EffectTarget.Friendly,
                    EffectModifier = EffectModifier.VisionRange,
                    CooldownTicks = 0,
                },
                new SpecialistEffectConfiguration
                {
                    Value = new EffectValue()
                    {
                        Value = 1,
                        ValueType = ValueType.Numeric,
                    },
                    EffectTrigger = EffectTrigger.SubEntersVisionRange,
                    EffectTarget = EffectTarget.Enemy,
                    EffectModifier = EffectModifier.RedirectEnemiesToSpecialistLocation,
                    CooldownTicks = 0,
                },
            },
            SpecialistClasses = { SpecialistClass.Protector, SpecialistClass.Warrior },
            PromotesFrom = null,
            OnlyActiveIfCaptured = false,
            NumberOnHire = 1,
        })
        {
        }
    }
}