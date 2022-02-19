using SubterfugeCore.Core.Components;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.Entities.Specialists;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core.GameEvents.NaturalGameEvents
{
    /// <summary>
    /// Friendly sub arrival
    /// </summary>
    public class FriendlySubArrive : NaturalGameEvent
    {
        Sub _arrivingSub;
        Outpost _outpost;

        /// <summary>
        /// Friendly sub arrival event
        /// </summary>
        /// <param name="combatant1">Combatant 1</param>
        /// <param name="combatant2">Combatant 2</param>
        /// <param name="occursAt">Tick of sub arrival</param>
        public FriendlySubArrive(Entity combatant1, Entity combatant2, GameTick occursAt) : base(occursAt, Priority.NATURAL_PRIORITY_9)
        {
            this._arrivingSub = (Sub)(combatant1 is Sub ? combatant1 : combatant2);
            this._outpost = (Outpost)(combatant1 is Outpost ? combatant1 : combatant2);
        }

        /// <summary>
        /// Undoes the sub's arrival
        /// </summary>
        /// <returns>If the event was undone</returns>
        public override bool BackwardAction(TimeMachine timeMachine,  GameState state)
        {
            if (base.EventSuccess)
            {
                this._outpost.GetComponent<DrillerCarrier>().RemoveDrillers(this._arrivingSub.GetComponent<DrillerCarrier>().GetDrillerCount());
                this._outpost.GetComponent<SpecialistManager>()
                    .RemoveSpecialists(this._arrivingSub.GetComponent<SpecialistManager>().GetSpecialists());
                state.AddSub(this._arrivingSub);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Perfoms a friendly sub arrival
        /// </summary>
        /// <returns>If the event was successful</returns>
        public override bool ForwardAction(TimeMachine timeMachine, GameState state)
        {
            if (state.SubExists(this._arrivingSub) && state.OutpostExists(this._outpost))
            {
                this._outpost.GetComponent<DrillerCarrier>().AddDrillers(this._arrivingSub.GetComponent<DrillerCarrier>().GetDrillerCount());
                this._outpost.GetComponent<SpecialistManager>().AddSpecialists(this._arrivingSub.GetComponent<SpecialistManager>().GetSpecialists());
                state.RemoveSub(this._arrivingSub);
                base.EventSuccess = true;
            }
            else
            {
                base.EventSuccess = false;
            }
            return base.EventSuccess;
        }
        
        /// <summary>
        /// Determines if the event was successful.
        /// </summary>
        /// <returns>If the event is successful</returns>
        public override bool WasEventSuccessful()
        {
            return base.EventSuccess;
        }
	}
}
