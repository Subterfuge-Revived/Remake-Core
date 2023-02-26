using System.Collections.Generic;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Entities.Specialists;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents
{
    public class SpecialistCaptureEffect : NaturalGameEvent
    {
        private IEntity _combatant1;
        private IEntity _combatant2;

        private List<Specialist> combatant1Specialists;
        private List<Specialist> combatant2Specialists;
        private IEntity? winner;
        
        public SpecialistCaptureEffect(
            GameTick occursAt,
            IEntity combatant1,
            IEntity combatant2
        ) : base(occursAt, Priority.COMBAT_RESOLVE)
        {
            _combatant1 = combatant1;
            _combatant2 = combatant2;
        }

        public override bool ForwardAction(TimeMachine timeMachine, GameState state)
        {
            combatant1Specialists = _combatant1.GetComponent<SpecialistManager>().GetSpecialists();
            combatant2Specialists = _combatant2.GetComponent<SpecialistManager>().GetSpecialists();

            winner = GetWinner();
            if (winner != null)
            {
                var loser = winner == _combatant1 ? _combatant2 : _combatant1;
                loser.GetComponent<SpecialistManager>().CaptureAll();
            }
            else
            {
                // If no winner, all specialists die.
                _combatant1.GetComponent<SpecialistManager>().KillAll();
                _combatant2.GetComponent<SpecialistManager>().KillAll();
            }

            return true;
        }

        public override bool BackwardAction(TimeMachine timeMachine, GameState state)
        {
            if (winner != null)
            {
                var loser = winner == _combatant1 ? _combatant2 : _combatant1;
                loser.GetComponent<SpecialistManager>().UncaptureAll();
            }
            else
            {
                _combatant1.GetComponent<SpecialistManager>().AddFriendlySpecialists(combatant1Specialists);
                _combatant2.GetComponent<SpecialistManager>().AddFriendlySpecialists(combatant2Specialists);
            }

            return true;
        }

        public override bool WasEventSuccessful()
        {
            return true;
        }

        private IEntity? GetWinner()
        {
            var combatantOneDrillerCount = _combatant1.GetComponent<DrillerCarrier>().GetDrillerCount();
            var combatantTwoDrillerCount = _combatant2.GetComponent<DrillerCarrier>().GetDrillerCount();
            if (combatantOneDrillerCount <= 0 && combatantTwoDrillerCount <= 0)
            {
                return null;
            }

            if (combatantOneDrillerCount <= 0)
            {
                return _combatant2;
            }

            return combatantTwoDrillerCount <= 0 ? _combatant1 : null;
        }
    }
}