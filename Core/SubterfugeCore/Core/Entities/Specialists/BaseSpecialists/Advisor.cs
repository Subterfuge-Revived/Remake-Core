using SubterfugeCore.Core.Players;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public class Advisor : Specialist
    {
        public Advisor(Player owner) : base(owner, new SpecialistConfiguration()
        {
            Id = "Advisor",
            Priority = 1,
            SpecialistName = "Advisor",
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
                    EffectModifier = EffectModifier.SpecialistCapacity,
                    CooldownTicks = 0,
                },
                // TODO: Allow hiring from this specialist.
            },
            SpecialistClasses = { SpecialistClass.Investor, SpecialistClass.Influencer },
            PromotesFrom = null,
            OnlyActiveIfCaptured = false,
            NumberOnHire = 1,
        })
        {
        }
    }
}