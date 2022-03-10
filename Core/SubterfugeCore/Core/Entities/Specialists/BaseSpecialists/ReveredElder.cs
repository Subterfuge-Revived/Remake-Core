using SubterfugeCore.Core.Players;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public class ReveredElder : Specialist
    {
        public ReveredElder(Player owner) : base(owner, new SpecialistConfiguration()
        {
            Id = "ReveredElder",
            Priority = 1,
            SpecialistName = "Revered Elder",
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
                    EffectTarget = EffectTarget.All,
                    EffectModifier = EffectModifier.PreventSpecialistEffects,
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