using System;
using SubterfugeCore.Core.Entities.Specialists.Effects;
using SubterfugeCore.Core.Players;
using SubterfugeRemakeService;
using ValueType = SubterfugeRemakeService.ValueType;

namespace SubterfugeCore.Core.Entities.Specialists
{
    /// <summary>
    /// The queen specialist.
    /// </summary>
    public class Queen : Specialist
    {
        /// <summary>
        /// Creates an instance of a queen belonging to the player
        /// </summary>
        /// <param name="owner">The owner of the queen</param>
        public Queen(Player owner) : base(owner, new SpecialistConfiguration()
        {
            Id = "Queen",
            Priority = 1,
            SpecialistName = "Queen",
            Creator = new User()
            {
                Id = "0",
                Username = "Subterfuge"
            },
            SpecialistEffects =
            {
                // Kill the player if they lose combat with the queen
                new SpecialistEffectConfiguration
                {
                    Value = new EffectValue()
                    {
                        Value = 1.0f,
                        ValueType = ValueType.Numeric,
                    },
                    EffectTrigger = EffectTrigger.CombatLoss,
                    EffectTarget = EffectTarget.Friendly,
                    EffectModifier = EffectModifier.KillPlayer,
                    CooldownTicks = 0,
                },
                new SpecialistEffectConfiguration
                {
                    Value = new EffectValue()
                    {
                        Value = 20,
                        ValueType = ValueType.Numeric
                    },
                    EffectTrigger = EffectTrigger.ArriveOutpost,
                    EffectTarget = EffectTarget.Friendly,
                    EffectModifier = EffectModifier.ShieldMaxValue,
                    CooldownTicks = 0,
                },
                new SpecialistEffectConfiguration
                {
                    Value = new EffectValue()
                    {
                        Value = -20,
                        ValueType = ValueType.Numeric
                    },
                    EffectTrigger = EffectTrigger.LeaveOutpost,
                    EffectTarget = EffectTarget.Friendly,
                    EffectModifier = EffectModifier.ShieldMaxValue,
                    CooldownTicks = 0,
                },
            },
            SpecialistClasses = { SpecialistClass.Protector },
            OnlyActiveIfCaptured = false,
        })
        {
        }
    }
}