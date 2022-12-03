﻿using System.Collections.Generic;
using System.Linq;
using SubterfugeCore.Core.Components;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Models.GameEvents;

namespace SubterfugeCore.Core.Entities.Specialists.Effects
{
    public static class SpecialistExtensions
    {
        public static float GetEffectScalar(this SpecialistEffectScale effectScale, GameState.GameState state, Entity friendly, Entity enemy)
        {
            // TODO: Apply specialist scaling here
            // Note: The forward effect value and backwards effect values should provide the same scaling ratio.
            // However, calculating them may need some work as going forward & backwards are different scenarios
            // with different information avaliable to you.
            
            // ex: deal 10% of enemy drillers. Imagine sub has 100 originally.
            // Forward: You know: has 100, deal 10%
            // Going forward the calculation is: (100 - (100 * 0.10)) = 90
            // While this calculation makes sense, we cannot easily reverse this. This is because 'start' is unknown when
            // going backwards.
            // The equation becomes:
            // start - (start * scale) = end
            
            // Going backwards you only know the 'end' and 'scale' values. This makes it impossible to determine the
            // 'start' value.
            // Sure you could simplify the equation and solve it,
            // but we need this formula to be applicable to ALL scaling values.
            // We can't just go simplify every single scaling equation possible.
            // Thus, we need some consistency for determine how scaling effects are applied.
            // In order to ensure that the 'start' and 'end' value of an effect can be determined,
            // ===============================IMPORTANT===============================
            // ALL SCALING FORMULAS MUST BE IN THE FORM:
            // end = start * scale
            
            // This makes is so that going forwards we now have:
            // end = start * scale.
            // and going backwards we can rearrange to get:
            // start = end / scale
            
            // This makes it so that reversing an effect doesn't need to keep track of any previous values
            // of the effect to undo itself. This keeps memory usage lower as games get longer.
            
            // For example, repeating the scenario above:
            // A 100 driller sub takes 10% damage.
            // Instead of thinking about it as taking 10% damange, think of it as: "AlterDriller" effect, which
            // alters the enemy to "0.90x" their original amount. This results in:
            // end = 100 * 0.90
            // end = 90
            // And going back we have:
            // start = 90 / 0.90
            // start = 100
            // Perfectly reversible!!
            
            // This means that based on the effect, you may need to determine the forward & backward equations
            // based on if the effect is positive or negative.
            // Another scenario:
            // Heal 10% of your drillers after combat.
            // ex) end of combat you have 100 drillers.
            
            // We now know the formula has to be:
            // end = start * scale
            // so instead of "heal 10%" which looks like this:
            // end = 100 + (100 * 0.10),
            // we need to think of it as "AlterDriller" "Friendly" by "1.10x" on "Local" "CombatSuccess"
            // end = 100 * 1.10
            // end = 110
            // This lets us easily go backwards with:
            // start = 110 / 1.10
            // start = 100
            
            // This will need to be documented to players who are utilizing the scaling effects so that it is understood
            // how to configure a scaling effect. While you might think to put a scalar of 0.10 for 10% heal, instead
            // it should be thought of "AlterDrillers" by "1.10x". It would make sense to have the players think of
            // sentences for their effects like so: "ApplyEffectType" "Target" by "scale" on "eventRange" "event"

            if (effectScale.EffectScale == EffectScale.NoScale || effectScale.EffectScaleTarget == EffectTarget.NoTarget)
            {
                return 1;
            }

            float scalar;

            // Variable to store potential candidates for the scaling value.
            var candidates = new List<Entity>();
            switch (effectScale.EffectScaleTarget)
            {
                case EffectTarget.Friendly:
                    candidates.AddRange(state.GetPlayerTargetables(friendly.GetComponent<DrillerCarrier>().GetOwner()));
                    break;
                case EffectTarget.Enemy:
                    candidates.AddRange(state.GetPlayerTargetables(enemy.GetComponent<DrillerCarrier>().GetOwner()));
                    break;
                case EffectTarget.OnlyCombatParticipants:
                    candidates.AddRange(state.GetPlayerTargetables(enemy.GetComponent<DrillerCarrier>().GetOwner()));
                    candidates.AddRange(state.GetPlayerTargetables(friendly.GetComponent<DrillerCarrier>().GetOwner()));
                    break;
                case EffectTarget.Any:
                    foreach (var p in state.GetPlayers())
                    {
                        candidates.AddRange(state.GetPlayerTargetables(p));
                    }

                    break;
                case EffectTarget.NoTarget:
                default:
                    return 1;
            }
            
            // Filter out candidates based on the scale range.
            switch (effectScale.EffectTriggerRange)
            {
                case EffectTriggerRange.Local:
                    candidates = candidates.FindAll(x => x.GetComponent<PositionManager>().GetPositionAt(state.CurrentTick) == friendly.GetComponent<PositionManager>().GetPositionAt(state.CurrentTick));
                    break;
                case EffectTriggerRange.Self:
                    candidates = candidates.FindAll(x => x == friendly);
                    break;
                case EffectTriggerRange.ConstantRange:
                case EffectTriggerRange.LocationVisionRange:
                case EffectTriggerRange.PlayerVisionRange:
                case EffectTriggerRange.Global:
                    // TODO: Add sonar range filters
                    break;
                default:
                    return 1;
            }
            
            // Determine the count.
            /*
            switch (effectScale.EffectScale)
            {
                case EffectScale.PlayerDrillerCount:
                    scalar = candidates.Sum(x => x.GetComponent<DrillerCarrier>().GetDrillerCount());
                    break;
                case EffectScale.PlayerFactoryCount:
                    scalar = candidates.Select(c =>
                        c.GetType() == typeof(Outpost) && ((Outpost) c).GetOutpostType() == OutpostType.Factory).Count();
                    break;
                case EffectScale.PlayerMineCount:
                    scalar = candidates.Select(c =>
                        c.GetType() == typeof(Outpost) && ((Outpost) c).GetOutpostType() == OutpostType.Mine).Count();
                    break;
                case EffectScale.PlayerOutpostCount:
                    scalar = candidates.Select(c => c.GetType() == typeof(Outpost)).Count();
                    break;
                default:
                    return 1;
            }
            */
            
            return candidates.Select(c => c.GetType() == typeof(Outpost)).Count();;
        }
    }
}