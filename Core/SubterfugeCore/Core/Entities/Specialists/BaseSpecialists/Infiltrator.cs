using SubterfugeCore.Core.Players;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public class Infiltrator : Specialist
    {
        public Infiltrator(Player owner) : base(owner, new SpecialistConfiguration()
        {
            Id = "Infiltrator",
            Priority = 1,
            SpecialistName = "Infiltrator",
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
            },
            SpecialistClasses = { SpecialistClass.Piercer, SpecialistClass.Warrior },
            OnlyActiveIfCaptured = false,
        })
        {
        }
    }
}