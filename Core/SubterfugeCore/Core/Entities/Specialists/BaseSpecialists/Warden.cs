using SubterfugeCore.Core.Players;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public class Warden : Specialist
    {
        public Warden(Player owner) : base(owner, new SpecialistConfiguration()
        {
            Id = "Warden",
            Priority = 1,
            SpecialistName = "Warden",
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
            },
            SpecialistClasses = { SpecialistClass.Producer },
            PromotesFrom = null,
            OnlyActiveIfCaptured = false,
            NumberOnHire = 1,
        })
        {
        }
    }
}