using SubterfugeCore.Core.Players;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public class Saboteur : Specialist
    {
        public Saboteur(Player owner) : base(owner, new SpecialistConfiguration()
        {
            Id = "Saboteur",
            Priority = 1,
            SpecialistName = "Saboteur",
            Creator = new User()
            {
                Id = "0",
                Username = "Subterfuge"
            },
            SpecialistEffects =
            {
                // On combat, redirect enemy to their sender
                new SpecialistEffectConfiguration
                {
                    Value = new EffectValue()
                    {
                        Value = 1,
                        ValueType = ValueType.Numeric,
                    },
                    EffectTrigger = EffectTrigger.PostCombat,
                    EffectTarget = EffectTarget.Enemy,
                    EffectModifier = EffectModifier.ReturnToDestination,
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