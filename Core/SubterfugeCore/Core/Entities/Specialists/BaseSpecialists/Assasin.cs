using SubterfugeCore.Core.Players;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public class Assasin : Specialist
    {
        public Assasin(Player owner) : base(owner, new SpecialistConfiguration()
        {
            Id = "Assasin",
            Priority = 1,
            SpecialistName = "Assasin",
            Creator = new User()
            {
                Id = "0",
                Username = "Subterfuge"
            },
            SpecialistEffects =
            {
                // Kill all enemy specialists
                new SpecialistEffectConfiguration
                {
                    Value = new EffectValue()
                    {
                        Value = 1,
                        ValueType = ValueType.Numeric,
                    },
                    EffectTrigger = EffectTrigger.PostCombat,
                    EffectTarget = EffectTarget.Enemy,
                    EffectModifier = EffectModifier.KillSpecialists,
                    CooldownTicks = 0,
                },
            },
            SpecialistClasses = { SpecialistClass.Controller },
            PromotesFrom = null,
            OnlyActiveIfCaptured = false,
            NumberOnHire = 2,
        })
        {
        }
    }
}