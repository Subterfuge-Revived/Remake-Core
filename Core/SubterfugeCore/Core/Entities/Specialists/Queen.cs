using SubterfugeCore.Core.Entities.Specialists.Effects;
using SubterfugeCore.Core.Entities.Specialists.Effects.Enums;
using SubterfugeCore.Core.Players;

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
        public Queen(Player owner) : base("Queen", 0, owner)
        {
            // A specialist effect that will kill the player whenever this specialist loses a combat.
            this.AddSpecialistEffect(
                new SpecialistEffectFactory().createSpecialistEffect(
                    new SpecialistEffectConfiguration
                    {
                        EffectScale = EffectScale.None,
                        EffectTarget = EffectTarget.Friendly,
                        EffectTrigger = EffectTrigger.CombatLoss,
                        EffectTriggerRange = EffectTriggerRange.Self,
                        EffectType = EffectModifier.KillPlayer,
                        ScaleRange = EffectTriggerRange.Self,
                        ScaleTarget = EffectTarget.Friendly,
                        Value = 1
                    }
                )
            );
            
            // Adds bonus shields to an outpost when the specialist arrives at an outpost
            this.AddSpecialistEffect(
                new SpecialistEffectFactory().createSpecialistEffect(
                    new SpecialistEffectConfiguration
                    {
                        EffectScale = EffectScale.None,
                        EffectTarget = EffectTarget.Friendly,
                        EffectTrigger = EffectTrigger.SubArrive,
                        EffectTriggerRange = EffectTriggerRange.Self,
                        EffectType = EffectModifier.ShieldMaxValue,
                        ScaleRange = EffectTriggerRange.Self,
                        ScaleTarget = EffectTarget.Friendly,
                        Value = 20
                    }
                )
            );
            
            // Remove the bonus shields when the specialist leaves
            this.AddSpecialistEffect(
                new SpecialistEffectFactory().createSpecialistEffect(
                    new SpecialistEffectConfiguration
                    {
                        EffectScale = EffectScale.None,
                        EffectTarget = EffectTarget.Friendly,
                        EffectTrigger = EffectTrigger.SubLaunch,
                        EffectTriggerRange = EffectTriggerRange.Self,
                        EffectType = EffectModifier.ShieldMaxValue,
                        ScaleRange = EffectTriggerRange.Self,
                        ScaleTarget = EffectTarget.Friendly,
                        Value = -20
                    }
                )
            );
            // Create up to X 'SpecialistHireAvaliable' Events
        }
    }
}