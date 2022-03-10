using SubterfugeCore.Core.Players;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public class Magician : Specialist
    {
        public Magician(Player owner) : base(owner, new SpecialistConfiguration()
        {
            Id = "Magician",
            Priority = 1,
            SpecialistName = "Magician",
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
                        Value = 0.90f,
                        ValueType = ValueType.Percentage,
                    },
                    EffectTrigger = EffectTrigger.SubEntersVisionRange,
                    EffectTarget = EffectTarget.Enemy,
                    EffectModifier = EffectModifier.Speed,
                    CooldownTicks = 0,
                },
            },
            SpecialistClasses = { SpecialistClass.Zoner },
            PromotesFrom = null,
            OnlyActiveIfCaptured = false,
            NumberOnHire = 1,
        })
        {
        }
    }
}