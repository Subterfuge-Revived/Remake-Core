using System.Collections.Generic;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Entities.Specialists;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents
{
    public class CombatSpecialistCaptureEffect : PositionalGameEvent
    {
        public readonly IEntity Combatant1;
        public readonly IEntity Combatant2;

        private List<Specialist> combatant1Specialists;
        private List<Specialist> combatant2Specialists;
        private IEntity? winner;
        
        public CombatSpecialistCaptureEffect(
            GameTick occursAt,
            IEntity combatant1,
            IEntity combatant2
        ) : base(occursAt, Priority.COMBAT_RESOLVE, combatant1)
        {
            Combatant1 = combatant1;
            Combatant2 = combatant2;
        }

        public override bool ForwardAction(TimeMachine timeMachine)
        {
            combatant1Specialists = Combatant1.GetComponent<SpecialistManager>().GetSpecialists();
            combatant2Specialists = Combatant2.GetComponent<SpecialistManager>().GetSpecialists();

            winner = GetWinner();
            if (winner != null)
            {
                var loser = winner == Combatant1 ? Combatant2 : Combatant1;
                loser.GetComponent<SpecialistManager>().CaptureAllForward(timeMachine, loser);
            }
            else
            {
                // If no winner, all specialists die.
                Combatant1.GetComponent<SpecialistManager>().KillAll();
                Combatant2.GetComponent<SpecialistManager>().KillAll();
            }

            return true;
        }

        public override bool BackwardAction(TimeMachine timeMachine)
        {
            if (winner != null)
            {
                var loser = winner == Combatant1 ? Combatant2 : Combatant1;
                loser.GetComponent<SpecialistManager>().UncaptureAll(timeMachine, loser);
            }
            else
            {
                Combatant1.GetComponent<SpecialistManager>().ReviveAll(combatant1Specialists);
                Combatant2.GetComponent<SpecialistManager>().ReviveAll(combatant2Specialists);
            }

            return true;
        }

        public override bool WasEventSuccessful()
        {
            return true;
        }

        private IEntity? GetWinner()
        {
            var combatantOneDrillerCount = Combatant1.GetComponent<DrillerCarrier>().GetDrillerCount();
            var combatantTwoDrillerCount = Combatant2.GetComponent<DrillerCarrier>().GetDrillerCount();
            if (combatantOneDrillerCount <= 0 && combatantTwoDrillerCount <= 0)
            {
                return null;
            }

            if (combatantOneDrillerCount <= 0)
            {
                return Combatant2;
            }

            return combatantTwoDrillerCount <= 0 ? Combatant1 : null;
        }
    }
}