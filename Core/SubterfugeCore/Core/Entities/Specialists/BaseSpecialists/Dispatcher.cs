using SubterfugeCore.Core.Players;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public class Dispatcher : Specialist
    {
        public Dispatcher(Player owner) : base(owner, new SpecialistConfiguration()
        {
            Id = "Dispatcher",
            Priority = 1,
            SpecialistName = "Dispatcher",
            Creator = new User()
            {
                Id = "0",
                Username = "Subterfuge"
            },
            SpecialistEffects =
            {
                // Destroy 15 drillers on combat
                new SpecialistEffectConfiguration
                {
                    Value = new EffectValue()
                    {
                        Value = 1,
                        ValueType = ValueType.Numeric,
                    },
                    EffectTrigger = EffectTrigger.ArriveOutpost,
                    EffectTarget = EffectTarget.Friendly,
                    EffectModifier = EffectModifier.CanLaunchSubs,
                    CooldownTicks = 0,
                },
            },
            SpecialistClasses = { SpecialistClass.Influencer },
            PromotesFrom = null,
            OnlyActiveIfCaptured = false,
        })
        {
        }
    }
}