using SubterfugeCore.Core.Players;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public class Helmsman : Specialist
    {
        public Helmsman(Player owner) : base(owner, new SpecialistConfiguration()
        {
            Id = "Helmsman",
            Priority = 1,
            SpecialistName = "Helmsman",
            Creator = new User()
            {
                Id = "0",
                Username = "Subterfuge"
            },
            SpecialistEffects =
            {
                // Move 1.25x faster
                new SpecialistEffectConfiguration
                {
                    Value = new EffectValue()
                    {
                        Value = 1.25f,
                        ValueType = ValueType.Percentage,
                    },
                    EffectTrigger = EffectTrigger.LeaveOutpost,
                    EffectTarget = EffectTarget.Friendly,
                    EffectModifier = EffectModifier.Speed,
                    CooldownTicks = 0,
                },
            },
            SpecialistClasses = { SpecialistClass.Zoomer },
            PromotesFrom = null,
            OnlyActiveIfCaptured = false,
        })
        {
        }
    }
}