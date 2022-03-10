using SubterfugeCore.Core.Players;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public class Scrutineer : Specialist
    {
        public Scrutineer(Player owner) : base(owner, new SpecialistConfiguration()
        {
            Id = "Scrutineer",
            Priority = 1,
            SpecialistName = "Scrutineer",
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
                        Value = 2.0f,
                        ValueType = ValueType.Percentage,
                    },
                    EffectTrigger = EffectTrigger.ArriveOutpost,
                    EffectTarget = EffectTarget.Friendly,
                    EffectModifier = EffectModifier.ShieldRegeneration,
                    CooldownTicks = 0,
                },
            },
            SpecialistClasses = { SpecialistClass.Protector },
            PromotesFrom = null,
            OnlyActiveIfCaptured = false,
            NumberOnHire = 1,
        })
        {
        }
    }
}