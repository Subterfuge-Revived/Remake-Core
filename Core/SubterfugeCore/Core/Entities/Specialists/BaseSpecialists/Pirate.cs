using SubterfugeCore.Core.Players;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public class Pirate : Specialist
    {
        public Pirate(Player owner) : base(owner, new SpecialistConfiguration()
        {
            Id = "Pirate",
            Priority = 1,
            SpecialistName = "Pirate",
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
                    EffectModifier = EffectModifier.CanTargetEnemySubs,
                    CooldownTicks = 0,
                },
                new SpecialistEffectConfiguration
                {
                    Value = new EffectValue()
                    {
                        Value = 1.5f,
                        ValueType = ValueType.Percentage,
                    },
                    EffectTrigger = EffectTrigger.TargetSub,
                    EffectTarget = EffectTarget.Friendly,
                    EffectModifier = EffectModifier.Speed,
                    CooldownTicks = 0,
                },
            },
            SpecialistClasses = { SpecialistClass.Warrior, SpecialistClass.Zoomer },
            PromotesFrom = null,
            OnlyActiveIfCaptured = false,
            NumberOnHire = 1,
        })
        {
        }
    }
}