using SubterfugeCore.Core.Players;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public class Merchant : Specialist
    {
        public Merchant(Player owner) : base(owner, new SpecialistConfiguration()
        {
            Id = "Merchant",
            Priority = 1,
            SpecialistName = "Merchant",
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
                    EffectTrigger = EffectTrigger.HireSelf,
                    EffectTarget = EffectTarget.Friendly,
                    EffectModifier = EffectModifier.OfferedSpecialists,
                    CooldownTicks = 0,
                },
            },
            SpecialistClasses = { SpecialistClass.Investor, },
            PromotesFrom = null,
            OnlyActiveIfCaptured = false,
            NumberOnHire = 1,
        })
        {
        }
    }
}