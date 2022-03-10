using SubterfugeCore.Core.Players;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public class Hypnotist : Specialist
    {
        public Hypnotist(Player owner) : base(owner, new SpecialistConfiguration()
        {
            Id = "Hypnotist",
            Priority = 1,
            SpecialistName = "Hypnotist",
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
                    EffectTrigger = EffectTrigger.ArriveOutpost,
                    EffectTarget = EffectTarget.Friendly,
                    EffectModifier = EffectModifier.AllowMovingCapturedSpecialists,
                    CooldownTicks = 0,
                },
                new SpecialistEffectConfiguration
                {
                    Value = new EffectValue()
                    {
                        Value = 1,
                        ValueType = ValueType.Numeric,
                    },
                    EffectTrigger = EffectTrigger.ArriveOutpost,
                    EffectTarget = EffectTarget.Enemy,
                    EffectModifier = EffectModifier.ConvertSpecialistOwners,
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